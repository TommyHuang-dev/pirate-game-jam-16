using CameraShake;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Character : MonoBehaviour {
    private Animator _animator;
    private Collider2D _collider;
    private Rigidbody2D _rb;
    private SpriteRenderer _effects;
    private Camera camera;

    // list of collected advanced upgrades
    public Dictionary<AdvancedUpgrade, int> collectedUpgrades = new Dictionary<AdvancedUpgrade, int>();

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

    public enum DashState
    {
        None,
        Charging,
        Dashing,
        Recovery
    }
    [SerializeField] public DashState currentDashState = DashState.None;

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

    [Header("Combat Variables")]
    public int maxHealth;
    public int currentHealth;
    public HealthBar healthBar;

    public bool isInvincible = false;
    public float invincibilityDuration = 1.5f;
    public float invincibilityDeltaTime = 0.15f;


    #region Ranged Attack
    public GameObject projectile;
    public float attackRate, attackDamage;
    private float attackCooldown = 0; // remaining cooldown before next shot
    public float attackProjectileSpeed = 10.0f;
    public float attackProjectileDuration = 1.5f;
    #endregion

    [Header("Navigation")]
    private LevelLoader _levelLoader;

    [Header("Sprite animation")]
    private bool flipped = false;
    private void Awake() {
        SyncStats();
    }

    // Called before first frame update
    private void Start() {
        _rb = this.GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponent<Collider2D>();
        _levelLoader = FindFirstObjectByType<LevelLoader>();
        _effects = GetComponent<SpriteRenderer>();
        healthBar = FindFirstObjectByType<HealthBar>();

        if (camera == null) {
            camera = Camera.main; // Automatically assign the main camera if not set
        }

        // Turn off regular gravity so we can use a custom implementation
        _rb.gravityScale = 0;

        // Player entrance
        currentState = PlayerState.NoControl;
        StartCoroutine(EnterScene());
    }

    private void FixedUpdate() {
        #region attacc
        attackCooldown -= Time.fixedDeltaTime;
        healthBar.SetHealth(currentHealth);
        if ( // Determine if the player can attack
            Input.GetKey(KeyCode.Mouse0) 
            && attackCooldown <= 0f 
            && currentDashState != DashState.Charging 
            && currentDashState != DashState.Dashing
            && currentState != PlayerState.NoControl
            )
        {
            attackCooldown = 1.0f / attackRate;
            Vector3 mouseScreenPos = Input.mousePosition;
            Vector3 mouseWorldPos = camera.ScreenToWorldPoint(mouseScreenPos);
            var attackVelocity = mouseWorldPos - transform.position;
            attackVelocity.z = 0;
            attackVelocity.Normalize();
            attackVelocity *= attackProjectileSpeed;

            if (collectedUpgrades.ContainsKey(AdvancedUpgrade.Multishot))
            {
                spawnAttack(
                    new Vector2(attackVelocity.x, attackVelocity.y),
                    1 + 2 * collectedUpgrades[AdvancedUpgrade.Multishot],
                    60f / (2f + collectedUpgrades[AdvancedUpgrade.Multishot])
                );
            }
            else
            {
                spawnAttack(new Vector2(attackVelocity.x, attackVelocity.y));
            }
            //CameraShaker.Presets.ShortShake2D(positionStrength:0.04f, rotationStrength:0.05f);
            if (attackVelocity.x > 0.1f)
            {
                flipped = false;
            } else if (attackVelocity.x < -0.1f)
            {
                flipped = true;
            }
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
            _animator.SetFloat("speed", 0);
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
            _animator.SetFloat("speed", _rb.linearVelocity.magnitude);
            dashDuration.x -= Time.fixedDeltaTime;
            //_rb.linearVelocity = dashVector;
            if (dashDuration.x <= 0)
            {
                dashCooldown.x = dashCooldown.y;

                dashDrift.x = dashDrift.y;
                currentDashState = DashState.Recovery;
            }
        }
        #endregion

        // flip sprite depending on how its moving
        if (attackCooldown <= 0f)
        {
            if (_rb.linearVelocity.x > 0.1f) {
                flipped = false;
            } else if (_rb.linearVelocity.x < -0.1f)
            {
                flipped = true;
            
            }
        }
        if (flipped)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        } else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

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

    private void spawnAttack(Vector2 velocity, int num=1, float spread=0f)
    {
        var startOffset = (num - 1) * spread / 2;

        AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.Shoot, UnityEngine.Random.Range(0.9f, 1.2f), UnityEngine.Random.Range(0.8f, 1f));
        for (int i = 0; i < num; i++)
        {
            var angleOffset = startOffset - i * spread;

            float radians = angleOffset * Mathf.Deg2Rad; // Convert degrees to radians
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            var rotatedVelocity = new Vector2(
                velocity.x * cos - velocity.y * sin,
                velocity.x * sin + velocity.y * cos
            );

            if (projectile != null)
            {
                var spawnLocation = new Vector3(transform.position.x, transform.position.y, -5);
                var direction = rotatedVelocity.normalized;
                float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                GameObject spawnedObject = Instantiate(projectile, spawnLocation, Quaternion.Euler(0f, 0f, -angle));
                Rigidbody2D rb = spawnedObject.GetComponent<Rigidbody2D>();
                //if (SaveData.Instance.data.attackRate < 20) {
                //    AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.Shoot, UnityEngine.Random.Range(0.9f, 1.2f), UnityEngine.Random.Range(0.8f, 1f));
                //} else { } // Too annoying, figure it out later?
                rb.linearVelocity = rotatedVelocity;
                Destroy(spawnedObject, attackProjectileDuration);
            }
            else
            {
                Debug.LogWarning("Player has no projectile");
            }
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
        if (currentDashState == DashState.Charging && currentState != PlayerState.NoControl)
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
                _rb.linearVelocity = dashVector;

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
            var enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (enemyCount == 0)
            {
                var capillary = other.GetComponent<Collider2D>().GetComponent<Capillary>();
                SaveData.Instance.data.currentRoomCompleted = true;
                _levelLoader.LoadNextLevel(capillary.queuedScene ?? LevelLoader.SceneType.MainMenu);
            } else
            {
                Debug.Log("Went to the exit, but there are still " + enemyCount + " enemies alive!");
            }
        }

        // Check if the other object is an enemy
        if (currentDashState == DashState.Dashing && other.CompareTag("Enemy"))
        {
            Debug.Log("Player ate enemy");
            // Add logic to damage the enemy
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && !enemy.isBoss) // Boss logic handled in onCollision
            {
                Debug.Log("Applying " + dashDamage + " damage");
                enemy.ApplyDamage(dashDamage); // Example damage value
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        if (enemy != null) {
            if (currentDashState == DashState.Dashing) {
                AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.EnemyHit, UnityEngine.Random.Range(0.9f, 1.2f), UnityEngine.Random.Range(0.8f, 1f));
            }
        }
        if (enemy != null && enemy.isBoss) {
            CameraShaker.Presets.ShortShake2D(positionStrength: 0.08f, rotationStrength: 0.05f);
            Debug.Log("Applying " + dashDamage + " damage");
            enemy.ApplyDamage(dashDamage); // Example damage value
        }
    }
    #endregion


    #region Taking Damage
    public void ApplyDamage(int amount) {
        if (isInvincible) { return;  }
        if (currentDashState != DashState.Dashing) {
            AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.PlayerHurt, UnityEngine.Random.Range(0.9f, 1.2f), UnityEngine.Random.Range(0.8f, 1f));

            Debug.Log("Taking " + amount + " damage. HP: " + currentHealth + " -> " + (currentHealth - amount));
            currentHealth -= amount;
            SaveData.Instance.data.currentHealth -= amount;

            if (currentHealth <= 0) {
                Die();
            } else {
                StartCoroutine(Invincibility());
            }
        }
    }

    private IEnumerator Invincibility() {
        isInvincible = true;
        for (float i = 0; i < invincibilityDuration; i += invincibilityDeltaTime) {
            _effects.color = new Color(255, 255, 255, 0);
            yield return new WaitForSeconds(invincibilityDeltaTime);
            _effects.color = Color.white;
            yield return new WaitForSeconds(invincibilityDeltaTime);
        }
        isInvincible = false;
    }

    private void Die() {
        StartCoroutine(Lose());
    }

    private IEnumerator Lose() {
        _effects.color = Color.red;
        AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.PlayerHurt, 0.7f, 1.4f);
        AudioManager.Instance.PlayWinLoss(false, (int)_levelLoader.currentScene);
        currentState = PlayerState.NoControl;
        _collider.enabled = false;
        yield return new WaitForSeconds(1f);
        _effects.color = new Color(0, 0, 0, 0);     
        yield return new WaitForSeconds(6f);
        _levelLoader.LoadNextLevel(0);
        SaveData.Instance.DeleteSaveData(); // ouch :(
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
        // Load in data when player spawns
        maxMoveSpeed = SaveData.Instance.data.moveSpeed;
        attackRate = SaveData.Instance.data.attackRate;
        attackDamage = SaveData.Instance.data.attackDamage;
        maxHealth = SaveData.Instance.data.maxHealth;
        currentHealth = SaveData.Instance.data.currentHealth;

        collectedUpgrades = SaveData.Instance.data.collectedUpgrades;
    }
}