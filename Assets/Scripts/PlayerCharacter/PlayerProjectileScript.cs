using UnityEngine;

public class PlayerProjectileScript : MonoBehaviour
{
    public int damage = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Projectile hit SOMETHING");
        // Check if the other object is an enemy
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Projectile hit enemy");
            // Add logic to damage the enemy
            RBacteria_Chase enemy = other.GetComponent<RBacteria_Chase>();
            if (enemy != null)
            {
                Debug.Log("Applying " + damage + " damage");
                enemy.ApplyDamage(damage); // Example damage value
            }

            // Destroy the projectile after impact
            Destroy(gameObject);
        }
    }
}
