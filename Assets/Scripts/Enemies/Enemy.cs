using JetBrains.Annotations;
using System.Collections;
using System.Linq;
using UnityEngine;
using static Character;

// Enemy superclass. All enemies should inherit from this
public class Enemy : MonoBehaviour
{
    public bool isBoss = false;
    protected GameObject player;
    protected SpriteRenderer sprite;
    protected LevelLoader levelLoader;
    protected SpawnManager spawnManager;
    protected int enemyCount;
    [SerializeField] protected EnemyProjectile projectile;
    [SerializeField] protected BossProjectile bossProjectile;

    // all enemies should have these properties
    [SerializeField] public int health = 20;
    [SerializeField] protected float knockBackResistance = 0;
    [SerializeField] protected float maxSpeed = 2f;
    [SerializeField] protected float acceleration = 1f; // between 0 and 1. 1 means it instantly moves at max speed
    [SerializeField] protected int damage = 1;
    private bool canMove = false;
    private bool isDead = false;

    // for melee
    // nothing here lol

    // for ranged
    [SerializeField] protected float attackRate = 1.0f;  // Time between attacks
    protected float timeBtwShots;
    [SerializeField] protected Vector2 kiteDistance = new Vector2(5f, 6f); // stay within these bounds
    protected enum AIType
    {
        None,
        BasicMelee, // enemy will chase the player and melee attack
        BasicRanged, // enemy will attempt to kite the player and fire projectiles
        Custom
    }
    protected AIType type;

    protected float damageFlash = 0;  // set after taking damage, ticks down

    protected virtual void setType()
    {
        // use this to set the type of the enemy (idk a better way to do this)
    }

    private void Awake() {
        this.tag = "Enemy";
        sprite = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");
        levelLoader = FindFirstObjectByType<LevelLoader>();
        spawnManager = FindFirstObjectByType<SpawnManager>();

        setType();
        StartCoroutine(SpawnIn());
    }

    void MeleeScript()
    {
        // set up for distance and direction bacteria need to move to
        if (player.GetComponent<Character>().currentHealth <= 0)
        {
            float random = Random.Range(0f, 2 * Mathf.PI);
            Vector2 randomVec = new Vector2(Mathf.Cos(random) * 100, Mathf.Sin(random) * 100);
            transform.position = Vector2.Lerp(transform.position, Vector2.MoveTowards(this.transform.position, randomVec, (maxSpeed / 4f) * Time.deltaTime), acceleration);
            return;
        }
        // Calculate how to move to player and how fast
        transform.position = Vector2.Lerp(transform.position, Vector2.MoveTowards(this.transform.position, player.transform.position, maxSpeed * Time.deltaTime), acceleration);

        // TODO melee attack
    }
    public void RangedScript()
    {
        var playerPos = player.transform.position;
        var minDistance = kiteDistance.x;
        var maxDistance = kiteDistance.y;

        // If the player is dead, stop shooting and go in a random direction
        if (player.GetComponent<Character>().currentHealth <= 0)
        {
            float random = Random.Range(0f, 2 * Mathf.PI);
            Vector2 randomVec = new Vector2(Mathf.Cos(random) * 100, Mathf.Sin(random) * 100);
            transform.position = Vector2.MoveTowards(transform.position, randomVec, (maxSpeed / 4f) * Time.deltaTime);
            return;
        }

        if (Vector2.Distance(transform.position, playerPos) > maxDistance)
        {
            // move towards player
            transform.position = Vector2.Lerp(transform.position, Vector2.MoveTowards(transform.position, playerPos, maxSpeed * Time.deltaTime), acceleration);
        }
        else if (Vector2.Distance(transform.position, playerPos) < maxDistance && Vector2.Distance(transform.position, playerPos) > maxDistance)
        {
            // don't move
        }
        else if (Vector2.Distance(transform.position, playerPos) < minDistance)
        {
            // move away from player
            transform.position = Vector2.Lerp(transform.position, Vector2.MoveTowards(transform.position, playerPos, maxSpeed * Time.deltaTime * (-1)), acceleration);
        }


        if (timeBtwShots <= 0)
        {
            var projectileInstance = Instantiate(projectile, transform.position, Quaternion.identity);
            projectileInstance.damage = this.damage;
            timeBtwShots = attackRate;
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }
    }
    protected virtual void CustomBehaviour()
    {
        // override this if needed
    }

    private IEnumerator SpawnIn() {
        canMove = false;
        spawnManager.enemyCount++;
        yield return new WaitForSeconds(1.3f);
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (sprite == null) { Debug.LogWarning("null sprite!"); }
        if (!canMove) { // Putting this here for now but if we do stuns it'll probably have to be moved to an isSpawning tracker
            // Move up or down depending on spawn location
            Vector2 target;
            if (transform.position.y > 6) {
                target = new Vector2(Random.Range(transform.position.x - 2f, transform.position.x + 2f), transform.position.y - 10f);
            } else if (transform.position.y < -6) {
                target = new Vector2(Random.Range(transform.position.x - 2f, transform.position.x + 2f), transform.position.y + 10f);
            } else {
                target = Vector2.zero;
            }
            
            transform.position = Vector2.MoveTowards(
                transform.position,
                target,
                Random.Range(maxSpeed * 0.5f, maxSpeed * 2f) * Time.deltaTime
            );
            //Debug.Log("current " + transform.position + " target " + target);
            return;
        }

        if (type == AIType.BasicMelee)
        {
            MeleeScript();
        } else if (type == AIType.BasicRanged)
        {
            RangedScript();
        } else if (type == AIType.Custom)
        {
            CustomBehaviour();
        }
        
        // Sprite colour changes to indicate damage
        sprite.color = new Color(1, 1 - damageFlash, 1 - damageFlash);
        
        damageFlash = Mathf.Max(0, damageFlash - 4 * Time.deltaTime);
        DirectionFacing();
    }

    public virtual void ApplyDamage(int damage)
    {
        health -= damage;
        if (enemyCount < 5) {
            AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.EnemyHit, UnityEngine.Random.Range(0.8f, 1.3f), UnityEngine.Random.Range(0.3f, 0.4f));
        } else if (enemyCount < 15) {
            AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.EnemyHit, UnityEngine.Random.Range(0.8f, 1.3f), UnityEngine.Random.Range(0.1f, 0.2f));
        } else {
            AudioManager.Instance.PlaySFX(AudioManager.SoundEffects.EnemyHit, UnityEngine.Random.Range(0.8f, 1.3f), UnityEngine.Random.Range(0.05f, 0.075f));
        }

        if (health <= 0 && !isDead)
        {
            isDead = true;
            spawnManager.enemyCount--;
            Debug.Log(enemyCount + "enemies left after death");
            Die();
        }
        damageFlash = 0.5f;
    }
    // todo
    private void Die() {
        if (spawnManager.enemyCount == 0) {
            AudioManager.Instance.PlayWinLoss(true, (int)levelLoader.currentScene);
        }
        Destroy(gameObject);
    }
    public void ApplyKnockback(Vector2 knockback)
    {
        var knockbackWithZ = new Vector3(knockback.x, knockback.y, 0f);
        transform.position += knockbackWithZ;
    }
    void DirectionFacing()
    {
        Vector2 directionToTarget = (player.transform.position - transform.position).normalized;
        float dotProduct = Vector2.Dot(transform.right, directionToTarget);

        if (dotProduct > 0)
        {
            sprite.flipX = true;
        }
        else if (dotProduct < 0)
        {
            sprite.flipX = false;
        }
    }

    // TODO apply damage functions, etc.
    // maybe apply damage should return if it killed the enemy or not (useful if we want on-kill effects)
}
