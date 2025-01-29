using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.TimeZoneInfo;
using UnityEngine.SceneManagement;

public class Character : MonoBehaviour {
    private Animator _animator;
    private Collider2D _collider;
    private Rigidbody2D _rb;
    private Vector2 _respawnPoint;
    private Camera camera;

    [Header("Movement Variables")]
    public float maxMoveSpeed;
    public float acceleration = 2f; // value between 0 and 1 used by lerp (technically it could be higher than 1)
    public float accelerationMaxSpeedInc = 2.0f; // when moving the player, a higher acceleration value is used
    public float friction = 1.5f; // how fast the player slows down naturally if they stop accelerating

    //public Vector2 veldebug;

    public enum PlayerState
    {
        Stopped,
        Moving,  // still moving but actively accelerating
        Accelerating,
        Drifting, // still moving but not actively accelerating
        NoControl // in this state the PC can't be controlled
    }
    [SerializeField] public PlayerState currentState = PlayerState.Stopped;

    private enum DashState
    {
        None,
        Charging,
        Dashing,
        Recovery
    }
    [SerializeField] private DashState currentDashState = DashState.None;

    [Header("Dash Variables")]
    #region Dash Attack
    public int dashDamage = 10;
    // Variables during charge
    public Vector2 dashCharge = new Vector2(0.0f, 0.8f);
    public float dashChargePerSec = 1.0f;
    public float dashChargeSlow = 0.6f; // speed penalty while charging
    // Variables during dash
    public float dashMinDistance = 2.0f;
    public float dashMaxDistance = 8.0f;
    public Vector2 dashDuration = new Vector2(0.0f, 0.6f); // remaining / max duration
    // Variables during recovery and cooldown
    public Vector2 dashCooldown = new Vector2(0.0f, 0.4f); // remaining / max cooldown
    public Vector2 dashDrift = new Vector2(0.0f, 0.4f); // removes speed cap to allow for gradual slow down
    private Vector2 dashVector = new Vector2(0, 0);
    #endregion

    [Header("Ranged Attack Variables")]
    #region Ranged Attack
    public GameObject projectile;
    public float attackRate, attackDamage;
    private float attackCooldown = 0; // remaining cooldown before next shot
    public float attackProjectileSpeed = 10.0f;
    public float attackProjectileDuration = 1.5f;
    #endregion

    [Header("Navigation")]
    private LevelLoader _levelLoader;

    // Called before first frame update
    private void Start() {
        _rb = this.GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponent<Collider2D>();
        _levelLoader = FindFirstObjectByType<LevelLoader>();

        if (camera == null)
        {
            camera = Camera.main; // Automatically assign the main camera if not set
        }

        // Turn off regular gravity so we can use a custom implementation
        _rb.gravityScale = 0;

        // Player entrance
        currentState = PlayerState.NoControl;
        StartCoroutine(EnterScene());
    }
    private void Awake() {
        // Load in data when player spawns
        maxMoveSpeed = SaveData.Instance.data.moveSpeed;
        attackRate = SaveData.Instance.data.attackRate;
        attackDamage = SaveData.Instance.data.attackDamage;
    }

    private void FixedUpdate() {
        #region attacc
        attackCooldown -= Time.fixedDeltaTime;
        if (Input.GetKey(KeyCode.Mouse0) && attackCooldown <= 0f)
        {
            attackCooldown = 1.0f / attackRate;
            Vector3 mouseScreenPos = Input.mousePosition;
            Vector3 mouseWorldPos = camera.ScreenToWorldPoint(mouseScreenPos);
            var attackVelocity = mouseWorldPos - transform.position;
            attackVelocity.z = 0;
            attackVelocity.Normalize();
            attackVelocity *= attackProjectileSpeed;

            spawnAttack(new Vector2(attackVelocity.x, attackVelocity.y));
        }
        #endregion
        calcDashState();
        calcPlayerState();

        //targetHSpeed = Mathf.Lerp(_rb.linearVelocity.x, targetHSpeed, 1);
        //float hAccelRate = (Mathf.Abs(targetHSpeed) > 0.01f) ? runAcceleration : runDeceleration;
        //float hSpeedDiff = targetHSpeed - _rb.linearVelocity.x;

        #region movement
        if (currentState == PlayerState.Moving && currentDashState != DashState.Dashing) {
            // get values from keeb
            var targetVelocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            targetVelocity.Normalize();
            targetVelocity *= (maxMoveSpeed + accelerationMaxSpeedInc) * (currentDashState == DashState.Charging ? (1 - dashChargeSlow) : 1);
            targetVelocity = Vector2.Lerp(_rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

            //_rb.AddForce(movement, ForceMode2D.Force);
            _rb.linearVelocity = targetVelocity;

            var debugTargetVelocity = new Vector3(targetVelocity.x, targetVelocity.y, 0);
            Debug.DrawLine(transform.position, transform.position + debugTargetVelocity, Color.green);
        }

        // clamp velocity to max
        if (currentDashState != DashState.Dashing && currentDashState != DashState.Recovery) {
            var tempMaxSpeed = maxMoveSpeed;
            if (currentDashState == DashState.Charging)
            {
                tempMaxSpeed *= (1 - dashChargeSlow);
            }
            _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, tempMaxSpeed);
        } 
        else if (currentDashState == DashState.Recovery) {
            var tempMaxSpeed = maxMoveSpeed + Mathf.Max(0, (dashVector.magnitude - maxMoveSpeed) * (dashDrift.x / dashDrift.y));
            _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, tempMaxSpeed);
        }

        // dash movement
        if (currentDashState == DashState.Dashing)
        {
            dashDuration.x -= Time.fixedDeltaTime;
            _rb.linearVelocity = dashVector;
            if (dashDuration.x <= 0)
            {
                dashCooldown.x = dashCooldown.y;

                dashDrift.x = dashDrift.y;
                currentDashState = DashState.Recovery;
            }
            // TODO damage on enemy contact
        }
        #endregion

        #region Friction
        // if the player isn't actively accelerating, add friction
        if (currentState == PlayerState.Stopped)
        {
            _rb.linearVelocity.Set(0, 0);
        }
        else if (currentState != PlayerState.Moving && currentDashState != DashState.Dashing)
        {
            float frictionAmount = Mathf.Max(_rb.linearVelocity.magnitude * friction, friction);
            if (frictionAmount * Time.fixedDeltaTime > _rb.linearVelocity.magnitude)
            {
                _rb.linearVelocity.Set(0, 0);
            }
            var direction = _rb.linearVelocity;
            direction.Normalize();

            _rb.linearVelocity += -frictionAmount * direction * Time.fixedDeltaTime;
        }
        #endregion
    }

    private void spawnAttack(Vector2 velocity)
    {
        if (projectile != null)
        {
            var spawnLocation = new Vector3(transform.position.x, transform.position.y, -5);
            var direction = velocity.normalized;
            spawnLocation.x += direction.x * 0.5f;
            spawnLocation.y += direction.y * 0.5f;
            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            GameObject spawnedObject = Instantiate(projectile, spawnLocation, Quaternion.Euler(0f, 0f, -angle));
            Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();

            rb.linearVelocity = velocity;
            Destroy(spawnedObject, attackProjectileDuration);
        }
        else
        {
            Debug.LogWarning("Player has no projectile");
        }
            
    }

    private void calcDashState()
    {
        #region Dash Calcs
        if (currentDashState == DashState.None || currentDashState == DashState.Recovery)
        {
            dashCooldown.x -= Time.deltaTime;
            if (currentState != PlayerState.NoControl && Input.GetKey(KeyCode.Space) && dashCooldown.x <= 0)
            {
                dashCharge.x = 0.0f;
                currentDashState = DashState.Charging;
            }
        }
        if (currentDashState == DashState.Charging)
        {
            // change state to dash and set dash vector if button released
            dashCharge.x = Mathf.Min(dashCharge.y, dashCharge.x + Time.deltaTime);
            if (!Input.GetKey(KeyCode.Space))
            {
                var dashDistance = dashMinDistance + (dashMaxDistance - dashMinDistance) * (dashCharge.x / dashCharge.y);
                var dashSpeed = dashDistance / dashDuration.y;

                Vector3 mouseScreenPos = Input.mousePosition;
                Vector3 mouseWorldPos = camera.ScreenToWorldPoint(mouseScreenPos);
                var dashDirection = mouseWorldPos - transform.position;
                dashDirection.z = 0;
                dashDirection.Normalize();
                dashVector = dashDirection * dashSpeed;

                dashDuration.x = dashDuration.y;
                currentDashState = DashState.Dashing;

                // DEBUG STUFF
                Debug.DrawLine(transform.position, transform.position + (dashDuration.y * dashSpeed * dashDirection), Color.cyan, dashDuration.y);
                // ***
            }
            // DEBUG STUFF ***
            else
            {
                var dashDistance = dashMinDistance + (dashMaxDistance - dashMinDistance) * (dashCharge.x / dashCharge.y);
                var dashSpeed = dashDistance / dashDuration.y;
                Vector3 mouseScreenPos = Input.mousePosition;
                Vector3 mouseWorldPos = camera.ScreenToWorldPoint(mouseScreenPos);
                var dashDirection = mouseWorldPos - transform.position;
                dashDirection.z = 0;
                dashDirection.Normalize();
                Debug.DrawLine(transform.position, transform.position + (dashDuration.y * dashSpeed * dashDirection), Color.white);
            }
            // ***
        }
        if (currentDashState == DashState.Recovery)
        {
            dashDrift.x -= Time.deltaTime;
            if (dashDrift.x <= 0)
            {
                currentDashState = DashState.None;
            }
        }
        #endregion
    }

    private void calcPlayerState()
    {
        if (currentState == PlayerState.NoControl) { return; }
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.01f && Mathf.Abs(Input.GetAxis("Vertical")) < 0.01f)
        {
            if (_rb.linearVelocity.magnitude < 0.01f)
            {
                currentState = PlayerState.Stopped;
            }
            else
            {
                currentState = PlayerState.Drifting;
            }
        }
        else 
        {
            currentState = PlayerState.Moving;
        }
    }

    #region Collision Handling
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("RoomExit")) { 
            // TODO: Check if room exit is open. Could probably check theres nothing left with the tag "Enemy"
            var capillary = other.GetComponent<Collider2D>().GetComponent<Capillary>();
            Debug.Log("Going to a " + capillary.queuedScene.ToString());
            _levelLoader.LoadNextLevel(capillary.queuedScene ?? LevelLoader.SceneType.MainMenu);
        }

        // Check if the other object is an enemy
        if (currentDashState == DashState.Dashing && other.CompareTag("Enemy"))
        {
            Debug.Log("Player ate enemy");
            // Add logic to damage the enemy
            RBacteria_Chase enemy = other.GetComponent<RBacteria_Chase>();
            if (enemy != null)
            {
                Debug.Log("Applying " + dashDamage + " damage");
                enemy.ApplyDamage(dashDamage); // Example damage value
            }
        }
    }
    #endregion
    
    private IEnumerator EnterScene() {
        // Play animation, then wait 2 seconds and let the player play.
        var actualMaxMoveSpeed = maxMoveSpeed;
        maxMoveSpeed = 6.5f; // A slightly faster fall.
        currentState = PlayerState.NoControl;
        _rb.AddForce(-10.0f * Vector2.up, ForceMode2D.Impulse);
        yield return new WaitForSeconds(1.3f);
        currentState = PlayerState.Stopped;
        maxMoveSpeed = actualMaxMoveSpeed;
    }

    public void SyncStats()
    {
        Awake();
    }
}