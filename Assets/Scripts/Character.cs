using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    private Animator _animator;
    private Collider2D _collider;
    private Rigidbody2D _rb;
    private Vector2 _respawnPoint;

    [Header("Movement Variables")]
    public float maxMoveSpeed = 5.0f;
    public float acceleration = 0.9f; // value between 0 and 1 used by lerp
    public float accelerationMaxSpeedInc = 2.0f; // when moving the player, a higher acceleration value is used
    public float deceleration = 4.0f; // speed at which the player can actively stop and change direction. Currently not doing anything
    public float friction = 0.05f; // how fast the player slows down naturally if they stop accelerating

    //public Vector2 currentVelocity = new Vector2(0.0f, 0.0f); // should probably use rb velocity instead?

    private enum PlayerState
    {
        Stopped,
        Moving,  // still moving but actively accelerating
        Accelerating,
        Drifting, // still moving but not actively accelerating
        NoControl // in this state the PC can't be controlled
    }
    [SerializeField] private PlayerState currentState = PlayerState.Stopped;

    private static float globalGravity = -9.81f;
    private float gravityScale = 0.0f;  // default to no gravity since we are "suspended" in fluid

    public Vector2 movement;
    public Vector2 veldebug;


    // Called before first frame update
    private void Start() {
        _rb = this.GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponent<Collider2D>();

        // Set respawn point to initial position
        //SetRespawnPoint(transform.position);

        // Turn off regular gravity so we can use a custom implementation
        _rb.gravityScale = 0;
    }
    // Called once per frame
    private void Update() {
        //if (currentState == State.NoControl) {
        //    NoControl = 0;
        //    return;
        //}

        //// Jump check
        //if (Input.GetKeyDown(KeyCode.Space)) {
        //    Jump();
        //}

        //// Fell off map check
        //if (_rb.position.y < -20) {
        //    Die();
        //}

        //// Clamp fall speed
        //if (_rb.linearVelocity.y < _maxFallSpeed) {
        //    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _maxFallSpeed);
        //}
    }
    private void FixedUpdate() {
        // update the current player state if needed
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.0001f && Mathf.Abs(Input.GetAxis("Vertical")) < 0.0001f) {
            if (_rb.linearVelocity.magnitude < 0.0001f) {
                currentState = PlayerState.Stopped;
            } else {
                currentState = PlayerState.Drifting;
            }
        } else if (currentState != PlayerState.NoControl)
        {
            currentState = PlayerState.Moving;
        }

        // get values from keeb
        var targetVelocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        targetVelocity.Normalize();
        targetVelocity *= (maxMoveSpeed + accelerationMaxSpeedInc);
        var targetVelocityLerp = Vector2.Lerp(_rb.linearVelocity, targetVelocity, acceleration);
        var velocityDiff = targetVelocityLerp - _rb.linearVelocity;

        //targetHSpeed = Mathf.Lerp(_rb.linearVelocity.x, targetHSpeed, 1);
        //float hAccelRate = (Mathf.Abs(targetHSpeed) > 0.01f) ? runAcceleration : runDeceleration;
        //float hSpeedDiff = targetHSpeed - _rb.linearVelocity.x;

        #region Friction
        // if the player isn't actively accelerating, add friction
        if (currentState != PlayerState.Moving) {
            float frictionAmount = Mathf.Min(Mathf.Abs(_rb.linearVelocity.magnitude), Mathf.Abs(friction));
            var direction = _rb.linearVelocity;
            direction.Normalize();
            _rb.AddForce(-frictionAmount * direction, ForceMode2D.Impulse);
        }
        #endregion

        var movement = velocityDiff * acceleration;
        _rb.AddForce(movement, ForceMode2D.Force);

        // clamp velocity to max
        _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, maxMoveSpeed);

        //// DEBUG
        //print(targetVelocity + " t");
        print(_rb.linearVelocity + " v");
        //print(hMovement + " m");
        //print(hSpeedDiff + " d");
        //print(hAccelRate + " a");
        //// For animator on movement update
        //_animator.SetFloat("speed", _rb.linearVelocity.magnitude);
        veldebug = _rb.linearVelocity;
    }

    //#region Movement
    //private void Jump()
    //{
    //    //_rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    //}
    //#endregion

    //#region Dying!
    //private void SetRespawnPoint(Vector2 point)
    //{
    //    _respawnPoint = point;
    //}

    //private void MiniJump()
    //{
    //    _rb.AddForce(6f * Vector2.up, ForceMode2D.Impulse);
    //}
    //public void Die()
    //{
    //    _canMove = false;
    //    _collider.enabled = false;
    //    MiniJump();
    //    StartCoroutine(Respawn());
    //}

    //private IEnumerator Respawn()
    //{
    //    yield return new WaitForSeconds(1f);
    //    _rb.linearVelocity = new Vector2(0f, 0f);
    //    transform.position = _respawnPoint;
    //    _canMove = true;
    //    _collider.enabled = true;
    //}
    //#endregion
}