using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCharger : Enemy
{
    private Rigidbody2D _rb;

    [SerializeField] private float chargeDistance = 6f;
    [SerializeField] private float chargeSpeed = 10f;

    [SerializeField] private int numCharges = 3;  // charges in a row
    private Vector2 chargeCD = new Vector2(1.5f, 2.5f);
    private float interval = 1.0f;  // time between two charges in a set
    private int chargesLeft = 0;

    public EventFactory upgrader;

    public enum State
    {
        Charging,
        Recovery
    }
    State state = State.Recovery;

    protected override void setType()
    {
        upgrader = FindFirstObjectByType<EventFactory>();
        upgrader.upgradeType = EventUpgradeType.Advanced;
        _rb = this.GetComponent<Rigidbody2D>();
        levelLoader = FindFirstObjectByType<LevelLoader>();
        isBoss = true;
        this.type = AIType.Custom;
    }

    protected override void CustomBehaviour()
    {
        chargeCD.x -= Time.deltaTime;
        // friction if not charging
        if (state == State.Recovery)
        {
            _rb.linearVelocity *= (1 - (Time.deltaTime * 0.8f));
            if (chargeCD.x <= 0)
            {
                chargesLeft = numCharges;
                state = State.Charging;
            }
        }
        if (state == State.Charging && chargeCD.x <= 0)
        {
            var directionDiff = player.transform.position - this.transform.position;
            var chargeVector = new Vector2(directionDiff.x, directionDiff.y).normalized;
            chargeVector.x += Random.Range(-0.1f, 0.1f);
            chargeVector.y += Random.Range(-0.1f, 0.1f);
            chargeVector *= chargeSpeed;
            _rb = this.GetComponent<Rigidbody2D>();
            _rb.linearVelocity = chargeVector;

            chargesLeft--;
            chargeCD.x = interval;
            if (chargesLeft <= 0)
            {
                state = State.Recovery;
                chargeCD.x = chargeCD.y;
            }
        }
        if (state == State.Charging && chargeCD.x <= interval * 0.2f)
        {
            _rb.linearVelocity *= (1 - (Time.deltaTime * 0.3f));
        }
    }

    public void OnCollisionEnter2D(Collision2D other) { // Since the boss has collision, use this instead
        Character player = other.gameObject.GetComponent<Character>();
        if (player != null && state == State.Charging)
        {
            player.ApplyDamage(damage);
        }
    }
}
