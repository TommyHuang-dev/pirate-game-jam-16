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
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null) {
                Debug.Log("Applying " + damage + " damage to enemy");
                enemy.ApplyDamage(damage);
                var knockbackDirection = enemy.transform.position - this.transform.position;
                knockbackDirection.z = 0;
                knockbackDirection.Normalize();
                enemy.ApplyKnockback(knockbackDirection * knockback);
            } else
            {
                Debug.LogError("Couldn't find the enemy object that was hit");
            }

            // Destroy the projectile after impact
            Destroy(gameObject);
        }
    }
}
