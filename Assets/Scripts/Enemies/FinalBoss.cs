using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Character;

public class FinalBoss : Enemy {
    private Rigidbody2D _rb;

    [SerializeField] private float chargeDistance = 6f;
    [SerializeField] private float chargeSpeed = 10f;
    

    [SerializeField] private int numCharges = 3;  // charges in a row
    private Vector2 chargeCD = new Vector2(1.5f, 2.5f);
    private float interval = 1.0f;  // time between two charges in a set
    private int chargesLeft = 0;

    public EventFactory upgrader;

    // Ranged
    private float timeBtwShots;
    private float projectileSpeed = 4f;
    // State
    private bool canMove;
    public enum State {
        Charging,
        Recovery
    }
    State state = State.Recovery;

    private void Start() {
        StartCoroutine(EnterScene());
    }

    protected override void setType() {
        upgrader = FindFirstObjectByType<EventFactory>();
        upgrader.upgradeType = EventUpgradeType.Advanced;
        _rb = this.GetComponent<Rigidbody2D>();
        levelLoader = FindFirstObjectByType<LevelLoader>();
        isBoss = true;
        this.type = AIType.Custom;
        health = 2500;
        attackRate = 0.5f;
    }

    protected override void CustomBehaviour() {
        if (!canMove) { return; }
        chargeCD.x -= Time.deltaTime;
        // friction if not charging
        if (state == State.Recovery) {
            _rb.linearVelocity *= (1 - (Time.deltaTime * 0.8f));
            if (chargeCD.x <= 0) {
                chargesLeft = numCharges;
                state = State.Charging;
            }
        }
        if (state == State.Charging && chargeCD.x <= 0) {
            var directionDiff = player.transform.position - this.transform.position;
            var chargeVector = new Vector2(directionDiff.x, directionDiff.y).normalized;
            chargeVector.x += Random.Range(-0.1f, 0.1f);
            chargeVector.y += Random.Range(-0.1f, 0.1f);
            chargeVector *= chargeSpeed;
            _rb = this.GetComponent<Rigidbody2D>();
            _rb.linearVelocity = chargeVector;

            chargesLeft--;
            chargeCD.x = interval;
            if (chargesLeft <= 0) {
                state = State.Recovery;
                chargeCD.x = chargeCD.y;
            }
        }
        if (state == State.Charging && chargeCD.x <= interval * 0.2f) {
            _rb.linearVelocity *= (1 - (Time.deltaTime * 0.3f));
        }

        FightRanged();
    }

    private void FightRanged() {
        var playerPos = player.transform.position;

        // If the player is dead, stop shooting and go in a random direction
        if (player.GetComponent<Character>().currentHealth <= 0) {
            float random = Random.Range(0f, 2 * Mathf.PI);
            Vector2 randomVec = new Vector2(Mathf.Cos(random) * 100, Mathf.Sin(random) * 100);
            transform.position = Vector2.MoveTowards(transform.position, randomVec, (maxSpeed / 4f) * Time.deltaTime);
            return;
        }

        if (timeBtwShots <= 0) {
            RandomShot();
            timeBtwShots = attackRate;
        } else {
            timeBtwShots -= Time.deltaTime;
        }
    }
    // Spawns projectiles from random locations.
    private void RandomShot() {
        Vector2 playerPos = player.transform.position;
        Vector2 bossPos = player.transform.position;

        float offsetX = Random.Range(-7f, 7f);
        float offsetY = Random.Range(-7f, 7f);
        Vector2 offsetVec = new Vector2(bossPos.x + offsetX, bossPos.y + offsetY);
        // Prevent projectiles from spawning in a 2 unit rectangle around the player.
        float playerBuffer = 7f;
        if (Vector2.Distance(playerPos, offsetVec) < playerBuffer)
        {
            offsetVec.x = bossPos.x + Random.Range(-7f, 7f);
            offsetVec.y = bossPos.y + Random.Range(-7f, 7f);
        }

        // If we're still inside the buffer, don't spawn a projectile
        if (Vector2.Distance(playerPos, offsetVec) < playerBuffer) {
            return;
        }
        var projectileInstance = Instantiate(
                                    projectile,
                                    new Vector2(transform.position.x + offsetX, transform.position.y + offsetY),
                                    Quaternion.identity
                                    );
        projectileInstance.damage = this.damage;
        projectileInstance.speed = projectileSpeed;
    }

    private IEnumerator EnterScene() {
        // Play animation, then wait 2 seconds and let the player play.
        canMove = false;
        yield return new WaitForSeconds(1.3f);
        canMove = true;
    }
    public void OnCollisionEnter2D(Collision2D other) { // Since the boss has collision, use this instead
        Character player = other.gameObject.GetComponent<Character>();
        if (player != null && state == State.Charging) {
            player.ApplyDamage(damage);
        }
    }
}
