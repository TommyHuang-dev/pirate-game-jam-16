using UnityEngine;


public class EnemyProjectile : MonoBehaviour
{
    public int damage = 5;
    public float speed;

    private Transform player;
    private Vector2 target;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; //Find Player Tag

        target = new Vector2(player.position.x, player.position.y); // Project moves towards where player was when projectile was spawned
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (transform.position.x == target.x && transform.position.y == target.y)
        {
            Destroy(gameObject);
        }
    }


    public void OnTriggerEnter2D(Collider2D other) {
        // Check if the other object is an enemy
        if (other.CompareTag("Player")) {
            Debug.Log("Enemy Projectile hit Player");
            // Damage the player on hit
            Character player = other.GetComponent<Character>();
            if (player != null) {
                Debug.Log("Applying " + damage + " damage");
                player.TakeDamage(damage); // Example damage value
            }

            // Destroy the projectile after impact
            Destroy(gameObject);
        }
    }
}

