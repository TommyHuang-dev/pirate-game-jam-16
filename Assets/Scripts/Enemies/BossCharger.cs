using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCharger : Enemy
{
    private Rigidbody2D _rb;
    private LevelLoader levelLoader;

    [SerializeField] private float numCharges = 3f;  // charges in a row
    [SerializeField] private float chargeDistance = 8f;
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float interval = 0.5f;  // time between two charges in a set
    [SerializeField] private float cooldown = 4.0f;  // cooldown between sets of charges

    private Vector2 testReset = new Vector2(0f, 2.0f);

    protected override void setType()
    {
        levelLoader = FindFirstObjectByType<LevelLoader>();
        isBoss = true;
        this.type = AIType.Custom;
    }

    protected override void CustomBehaviour()
    {
        testReset.x -= Time.deltaTime;
        if (testReset.x <= 0)
        {
            var directionDiff = player.transform.position - this.transform.position;
            var chargeVector = new Vector2(directionDiff.x, directionDiff.y).normalized;
            chargeVector *= chargeSpeed;
            _rb = this.GetComponent<Rigidbody2D>();
            _rb.linearVelocity = chargeVector;

            testReset.x = testReset.y;
        }
    }

    //public void OnTriggerEnter2D(Collider2D other)
    //{
    //    // Check if the other object is an player
    //    if (other.CompareTag("Player"))
    //    {
    //        // Damage the player on hit
    //        Character player = other.GetComponent<Character>();
    //        if (player != null)
    //        {
    //            Debug.Log("Applying " + damage + " damage");
    //            player.ApplyDamage(damage);
    //        }
    //    }
    //}
    public void OnCollisionEnter2D(Collision2D other) { // Since the boss has collision, use this instead
        // Check if the other object is an player
        Character player = other.gameObject.GetComponent<Character>();
        if (player != null && player.currentDashState != Character.DashState.Dashing) {
            // Damage the player on hit
            Debug.Log("Applying " + damage + " damage");
            player.ApplyDamage(damage);
        } else if (player != null) {
            damageFlash = 0.5f;
        }
    }

    public override void ApplyDamage(int damage)
    {
        health -= damage;
        AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.EnemyHit, UnityEngine.Random.Range(0.9f, 1.2f), UnityEngine.Random.Range(0.7f, 1f));
        if (health <= 0)
        {
            Die();
            Destroy(gameObject);
        }
        damageFlash = 0.5f;
    }
    
    private void Die() {
        AudioManager.Instance.PlayWinLoss(true, (int)levelLoader.currentScene);
    }
}
