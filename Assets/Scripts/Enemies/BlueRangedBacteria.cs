using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class BlueRangedBacteria : MonoBehaviour
{
    public float speed; // speed at which the enemy moves
    public float stoppingDistance; // distance to stop chasing
    public float kiteDistance; // distance to kite player at
    public int healthPoints = 10; // hp 
    private float distance; 

    private Transform player;
    private SpriteRenderer sprite; // of the enemy
    private float damageFlash = 0;  // set after taking damage, ticks down

    //public GameObject projectilePrefab; // The projectile to shoot
    //public Transform shootPoint;       // Where the projectile spawns

    private float timeBtwShots;
    public float attackRate;  // Time between attacks
    public GameObject projectile; 

    //public float projectileSpeed = 10f; // Speed of the projectile


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //player = GameObject.Find("Player");
        // Find the player GameObject (make sure it's tagged as "Player")
        player = GameObject.FindGameObjectWithTag("Player").transform;
        sprite = GetComponent<SpriteRenderer>();

        timeBtwShots = attackRate;
    }

    // Update is called once per frame
    void Update()
    {
        // If the player is dead, stop shooting and go in a random direction
        if (player.GetComponent<Character>().currentHealth <= 0) {
            float random = Random.Range(0f, 2 * Mathf.PI);
            Vector2 randomVec = new Vector2(Mathf.Cos(random) * 100, Mathf.Sin(random) * 100);
            transform.position = Vector2.MoveTowards(this.transform.position, randomVec, (speed / 4f) * Time.deltaTime);
            return;
        }

        DirectionFacing();

        if (Vector2.Distance(transform.position, player.position) > stoppingDistance)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, player.position, speed * Time.deltaTime);
        }
        else if (Vector2.Distance(transform.position, player.position) < stoppingDistance && Vector2.Distance(transform.position, player.position) > kiteDistance) {
            transform.position = this.transform.position;
        }
        else if(Vector2.Distance(transform.position, player.position) < kiteDistance)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, player.position, speed * Time.deltaTime*(-1));
        }


        if(timeBtwShots <= 0) {
            Instantiate(projectile, transform.position, Quaternion.identity);
            timeBtwShots = attackRate;
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }
        // set up for distance and direction bacteria need to move to
        distance = Vector2.Distance(transform.position, player.transform.position);
        Vector2 direction = player.transform.position - transform.position;

        // Sprite colour changes to indicate damage
        sprite.color = new Color(1, 1 - damageFlash, 1 - damageFlash);
        damageFlash = Mathf.Max(0, damageFlash - 4 * Time.deltaTime);

        // normalize the vector so that it points to the player
        //direction.Normalize();
        //float angle = Mathf.Atan2(direction.y direction.x) * Mathf.Rad2Deg;


        //if (distance < distanceBetween && distance > 4)
        //{
        //    // Calculate how to move to player and how fast

        //    // Rotate
        //    //transform.rotation = Quaternion.Euler(Vector3.forward * angle);
        //}

        // Decrease the attack cooldown over time
        //attackCooldown -= Time.deltaTime;

        //if (player != null && attackCooldown <= 0f)
        //{
        //    AttackPlayer();
        //    attackCooldown = 1.0f / attackRate; // Reset cooldown
        //}

    }
    public void ApplyDamage(int damage)
    {
        healthPoints -= damage;
        if (healthPoints <= 0)
        {
            Destroy(gameObject);
        }
        damageFlash = 0.5f;
    }

    void DirectionFacing() {
        Vector2 directionToTarget = (player.transform.position - transform.position).normalized;
        float dotProduct = Vector2.Dot(transform.right, directionToTarget);

        if (dotProduct > 0) {
            sprite.flipX = true;
        } else if (dotProduct < 0) {
            sprite.flipX = false;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("PlayerProjectile"))
    //    {
    //        // Handle the hit (e.g., reduce health)
    //        healthPoints -= 5;
    //        if (healthPoints <= 0)
    //        {
    //            Destroy(gameObject);
    //        }
    //        Debug.Log("Enemy hit!");
    //    }
    //    Debug.Log("alsdlfajnslkdfjnalkjsdfn");
    //}
    //void AttackPlayer()
    //{
    //    // Calculate the direction to the player
    //    Vector3 attackDirection = (player.position - shootPoint.position).normalized;

    //    // Spawn the projectile
    //    GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

    //    // Apply velocity to projectile
    //    Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
    //    if (rb != null)
    //    {
    //        rb.linearVelocity = new Vector2(attackDirection.x, attackDirection.y) * projectileSpeed;
    //    }

    //    // Destroy the projectile after a certain time 
    //    Destroy(projectile, 5f);
    //}

}




