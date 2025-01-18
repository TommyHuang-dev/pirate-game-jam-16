using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    private Animator _animator;
    private Collider2D _collider;
    private Rigidbody2D _rb;
    private Vector2 _respawnPoint;

    [Header("Movement Variables")]
    public float horizontal;
    public float maxMoveSpeed = 1;
    public float turnSpeed = 30;
    public float runAcceleration;
    public float runDeceleration;
    public float friction;
    public float jumpForce = 10;

    public static float globalGravity = -9.81f;
    public float gravityScale = 1.0f;

    [SerializeField] private bool _canMove = true;
    private float _maxFallSpeed = -10;

    public Vector2 movement;
    public Vector2 veldebug;

    // Called before first frame update
    private void Start() {
        _rb = this.GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponent<Collider2D>();

        // Set respawn point to initial position
        SetRespawnPoint(transform.position);
        // Turn off regular gravity so we can use a custom implementation
        _rb.gravityScale = 0;
    }
    // Called once per frame
    private void Update() {
        if (!_canMove) {
            horizontal = 0;
            return;
        }

        // Jump check
        if (Input.GetKeyDown(KeyCode.Space)) {
            Jump();
        }

        // Fell off map check
        if (_rb.position.y < -20) {
            Die();
        }

        // Clamp fall speed
        if (_rb.linearVelocity.y < _maxFallSpeed) {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _maxFallSpeed);
        }
    }
    private void FixedUpdate() {
        // get values from keeb        
        float targetHSpeed = Input.GetAxis("Horizontal") * maxMoveSpeed;
        targetHSpeed = Mathf.Lerp(_rb.linearVelocity.x, targetHSpeed, 1);
        float hAccelRate = (Mathf.Abs(targetHSpeed) > 0.01f) ? runAcceleration : runDeceleration;
        float hSpeedDiff = targetHSpeed - _rb.linearVelocity.x;



        #region Friction
        if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))) {
            float hAmount = Mathf.Min(Mathf.Abs(_rb.linearVelocity.x), Mathf.Abs(friction));
            hAmount *= Mathf.Sign(_rb.linearVelocity.x);
            _rb.AddForce(Vector2.right * -hAmount, ForceMode2D.Impulse);
        }
        #endregion

        #region Custom gravity
        Vector2 gravity = globalGravity * gravityScale * Vector2.up;
        // We're doing higher gravity after jump is released to improve jumping responsiveness.
        if (Input.GetKey(KeyCode.Space)) {
            _rb.AddForce(gravity, ForceMode2D.Force);
        } else {
            _rb.AddForce(gravity * 3, ForceMode2D.Force);
        }
        #endregion

        float hMovement = hSpeedDiff * hAccelRate;
        _rb.AddForce(hMovement * Vector2.right, ForceMode2D.Force);
        //// DEBUG
        //print(targetHSpeed + " t");
        //print(hMovement + " m");
        //print(hSpeedDiff + " d");
        //print(hAccelRate + " a");
        //// For animator on movement update
        //_animator.SetFloat("speed", _rb.linearVelocity.magnitude);
        veldebug = _rb.linearVelocity;
    }
    #region Movement
    private void Jump() {
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    #endregion

    #region Dying!
    private void SetRespawnPoint(Vector2 point) {
        _respawnPoint = point;
    }

    private void MiniJump() {
        _rb.AddForce(6f * Vector2.up, ForceMode2D.Impulse);
    }
    public void Die() {
        _canMove = false;
        _collider.enabled = false;
        MiniJump();
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn() {
        yield return new WaitForSeconds(1f);
        _rb.linearVelocity = new Vector2(0f, 0f);
        transform.position = _respawnPoint;
        _canMove = true;
        _collider.enabled = true;
    }
    #endregion
}