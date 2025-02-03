using UnityEngine;


public class EnemyProjectile : MonoBehaviour
{
    public int damage = 5;
    public float speed;

    private Transform player;
    private SpawnManager spawnManager;
    private Vector2 target;

    protected float lifetime = 3f;
    protected float elapsedTime = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; //Find Player Tag
        spawnManager = FindFirstObjectByType<SpawnManager>();
        target = new Vector2(player.position.x, player.position.y); // Project moves towards where player was when projectile was spawned
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > lifetime || spawnManager.enemyCount == 0)
        {
            Destroy(gameObject);
        }

        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (transform.position.x == target.x && transform.position.y == target.y)
        {
            Destroy(gameObject);
        }
        
    }


    public void OnTriggerEnter2D(Collider2D other) {
        // Check if the other object is the player
        if (other.CompareTag("Player")) {
            // Damage the player on hit
            Character player = other.GetComponent<Character>();
            if (player != null && player.currentDashState != Character.DashState.Dashing && !player.isInvincible) {
                player.ApplyDamage(damage); // Example damage value
            }

            // Destroy the projectile after impact
            Destroy(gameObject);
        }
    }
}

