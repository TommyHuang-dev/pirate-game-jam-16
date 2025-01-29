using UnityEngine;

public class PlayerProjectileScript : MonoBehaviour
{
    public int damage = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other object is an enemy
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Projectile hit enemy");
            // Add logic to damage the enemy
            RBacteria_Chase enemy = other.GetComponent<RBacteria_Chase>();
            BlueRangedBacteria enemy2 = other.GetComponent <BlueRangedBacteria>();
            if (enemy != null) {
                Debug.Log("Applying " + damage + " damage");
                enemy.ApplyDamage(damage); // Example damage value
            } else if (enemy2 != null) {
                enemy2.ApplyDamage(damage);
            }

            // Destroy the projectile after impact
            Destroy(gameObject);
        }
    }
}
