using UnityEngine;

// Enemy superclass. All enemies should inherit from this
public class Enemy : MonoBehaviour
{
    protected GameObject player;
    protected SpriteRenderer sprite;
    [SerializeField] protected EnemyProjectile projectile;

    // all enemies should have these properties
    [SerializeField] protected int health = 20;
    [SerializeField] protected float knockBackResistance = 0;
    [SerializeField] protected float maxSpeed = 2f;
    [SerializeField] protected float acceleration = 1f; // between 0 and 1. 1 means it instantly moves at max speed
    [SerializeField] protected int damage = 1;

    // for melee
    // nothing here lol

    // for ranged
    [SerializeField] float attackRate = 1.0f;  // Time between attacks
    private float timeBtwShots;
    [SerializeField] protected Vector2 kiteDistance = new Vector2(5f, 6f); // stay within these bounds
    protected enum AIType
    {
        None,
        BasicMelee, // enemy will chase the player and melee attack
        BasicRanged, // enemy will attempt to kite the player and fire projectiles
        Custom
    }
    protected AIType type;

    private float damageFlash = 0;  // set after taking damage, ticks down

    protected virtual void setType()
    {
        // use this to set the type of the enemy (idk a better way to do this)
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.tag = "Enemy";
        player = GameObject.FindGameObjectWithTag("Player");
        sprite = GetComponent<SpriteRenderer>();
        setType();
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
    void RangedScript()
    {
        var playerPos = player.transform.position;
        var maxDistance = kiteDistance.x;
        var stoppingDistance = kiteDistance.y;

        // If the player is dead, stop shooting and go in a random direction
        if (player.GetComponent<Character>().currentHealth <= 0)
        {
            float random = Random.Range(0f, 2 * Mathf.PI);
            Vector2 randomVec = new Vector2(Mathf.Cos(random) * 100, Mathf.Sin(random) * 100);
            transform.position = Vector2.MoveTowards(transform.position, randomVec, (maxSpeed / 4f) * Time.deltaTime);
            return;
        }

        if (Vector2.Distance(transform.position, playerPos) > stoppingDistance)
        {
            transform.position = Vector2.Lerp(transform.position, Vector2.MoveTowards(transform.position, playerPos, maxSpeed * Time.deltaTime), acceleration);
        }
        else if (Vector2.Distance(transform.position, playerPos) < stoppingDistance && Vector2.Distance(transform.position, playerPos) > maxDistance)
        {
            // don't move
        }
        else if (Vector2.Distance(transform.position, playerPos) < maxDistance)
        {
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

    // Update is called once per frame
    void Update()
    {
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

    public void ApplyDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        damageFlash = 0.5f;
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
