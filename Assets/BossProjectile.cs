using UnityEngine;


public class BossProjectile : MonoBehaviour
{
    public int damage = 5;
    public float speed = 5f;
    public float lifetime = 3f;
    private float elapsedTime = 0f;
    private Vector2 moveDirection; // Stores the direction of movement

    // Called when projectile is instantiated
    public void Initialize(Vector2 direction)
    {
        moveDirection = direction.normalized; // Store and normalize the direction
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > lifetime)
        {
            Destroy(gameObject);
        }

        // Move the projectile in the stored direction
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }


    public void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other object is the player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enemy Projectile hit Player");
            // Damage the player on hit
            Character player = other.GetComponent<Character>();
            if (player != null && player.currentDashState != Character.DashState.Dashing && !player.isInvincible)
            {
                Debug.Log("Applying " + damage + " damage");
                player.ApplyDamage(damage); // Example damage value
            }

            // Destroy the projectile after impact
            Destroy(gameObject);
        }
    }
}

