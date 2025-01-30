using UnityEngine;

public class PlayerProjectileScript : MonoBehaviour
{
    public int damage = 5;
    private float knockback = 0.1f;

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
                Debug.Log("Applying " + damage + " damage to enemy");
                enemy.ApplyDamage(damage); // Example damage value
                // knockback stuff (TODO temporary, should make this code better)
                var knockbackDirection = enemy.transform.position - this.transform.position;
                knockbackDirection.z = 0;
                knockbackDirection.Normalize();
                enemy.transform.position += knockbackDirection * knockback;
            }
            else if (enemy2 != null) {
                Debug.Log("Applying " + damage + " damage to enemy2");
                enemy2.ApplyDamage(damage);
                var knockbackDirection = enemy2.transform.position - this.transform.position;
                knockbackDirection.z = 0;
                knockbackDirection.Normalize();
                enemy2.transform.position += knockbackDirection * knockback;
            }

            // Destroy the projectile after impact
            Destroy(gameObject);
        }
    }
}
