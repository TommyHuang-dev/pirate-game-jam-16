using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss324 : Enemy
{
    public EventFactory upgrader;
    public int maximumHealth = 1000;

    public enum State
    {
        Phase1,
        Phase2
    }
    State state = State.Phase1;

    protected override void setType()
    {
        levelLoader = FindFirstObjectByType<LevelLoader>();
        isBoss = true;
        upgrader = FindFirstObjectByType<EventFactory>();
        upgrader.upgradeType = EventUpgradeType.Advanced;
        this.type = AIType.Custom;
        
    }

    protected override void CustomBehaviour()
    {
        RangedScriptt();

    }

    void RangedScriptt()
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

        //not going to work right now idk how to get hp
        if (health < (maximumHealth / 2)){
            state = State.Phase2;
        }

        if (state == State.Phase1)
        {
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
                Vector2 directionToPlayer = ((Vector2)playerPos - (Vector2)transform.position).normalized;

                // Instantiate middle projectile
                CreateProjectile(directionToPlayer);

                // Create rotated projectiles
                CreateProjectile(RotateVector2(directionToPlayer, 30f));  // 30 right
                CreateProjectile(RotateVector2(directionToPlayer, -30f)); // 30 left
                CreateProjectile(RotateVector2(directionToPlayer, 60f));  // 60 right
                CreateProjectile(RotateVector2(directionToPlayer, -60f)); // 60 left

                timeBtwShots = attackRate;
            }
            else
            {
                timeBtwShots -= Time.deltaTime;
            }
        }
        if (state == State.Phase2)
        {
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

                ShootProjectiles360();


                timeBtwShots = attackRate;
            }
            else
            {
                timeBtwShots -= Time.deltaTime;
            }
        }

    }
    void CreateBossProjectile(Vector2 direction)
    {
        var projectileInstance = Instantiate(bossProjectile, transform.position, Quaternion.identity);

        projectileInstance.damage = this.damage;

        // Rotate projectile to face its direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectileInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Call Initialize() to set movement direction
        projectileInstance.GetComponent<BossProjectile>().Initialize(direction);
    }

    void ShootProjectiles360()
    {
        for (float angle = 0f; angle < 360f; angle += 45f) // Shoots every 45 degrees
        {
            Vector2 direction = RotateVector2(Vector2.right, angle); // Get direction
            CreateBossProjectile(direction);
        }
    }

    void CreateProjectile(Vector2 direction)
    {
        var projectileInstance = Instantiate(projectile, transform.position, Quaternion.identity);
        projectileInstance.damage = this.damage;

        // Apply rotation so it faces the movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectileInstance.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Set projectile movement direction 
        projectileInstance.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileInstance.speed;
    }

    Vector2 RotateVector2(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    public void OnCollisionEnter2D(Collision2D other)
    { // Since the boss has collision, use this instead
        Character player = other.gameObject.GetComponent<Character>();
        if (player != null)
        {
            player.ApplyDamage(damage);
        }
    }
}





//using UnityEngine;
//using UnityEngine.U2D;
//using Unity.VisualScripting;
//using UnityEngine.UI;

//public class Boss324 : Enemy
//{
//    protected GameObject player;
//    protected SpriteRenderer sprite;
//    protected EnemyProjectile projectile;

//    public Slider healthBar;
//    // for ranged
//    float attackRate = 1.0f;  // Time between attacks
//    private float timeBtwShots;
//    protected Vector2 kiteDistance = new Vector2(5f, 6f); // stay within these bounds

//    protected override void setType()
//    {
//        this.type = AIType.Custom;
//    }

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        healthBar.value = health;
//        setType();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        var playerPos = player.transform.position;
//        var maxDistance = kiteDistance.x;
//        var stoppingDistance = kiteDistance.y;

//        // If the player is dead, stop shooting and go in a random direction
//        if (player.GetComponent<Character>().currentHealth <= 0)
//        {
//            float random = Random.Range(0f, 2 * Mathf.PI);
//            Vector2 randomVec = new Vector2(Mathf.Cos(random) * 100, Mathf.Sin(random) * 100);
//            transform.position = Vector2.MoveTowards(transform.position, randomVec, (maxSpeed / 4f) * Time.deltaTime);
//            return;
//        }

//        if (Vector2.Distance(transform.position, playerPos) > stoppingDistance)
//        {
//            transform.position = Vector2.Lerp(transform.position, Vector2.MoveTowards(transform.position, playerPos, maxSpeed * Time.deltaTime), acceleration);
//        }
//        else if (Vector2.Distance(transform.position, playerPos) < stoppingDistance && Vector2.Distance(transform.position, playerPos) > maxDistance)
//        {
//            // don't move
//        }
//        else if (Vector2.Distance(transform.position, playerPos) < maxDistance)
//        {
//            transform.position = Vector2.Lerp(transform.position, Vector2.MoveTowards(transform.position, playerPos, maxSpeed * Time.deltaTime * (-1)), acceleration);
//        }


//        if (timeBtwShots <= 0)
//        {
//            var projectileInstance = Instantiate(projectile, transform.position, Quaternion.identity);
//            projectileInstance.damage = this.damage;
//            timeBtwShots = attackRate;
//        }
//        else
//        {
//            timeBtwShots -= Time.deltaTime;
//        }
//    }
//}
