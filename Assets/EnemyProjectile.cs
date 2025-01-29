using UnityEngine;


public class EnemyProjectile : MonoBehaviour
{
    public int damage;
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
            DestroyEnemyProjectile();
        }
    }

    void DestroyEnemyProjectile()
    {
        Destroy(gameObject);
    }

    void onTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DestroyEnemyProjectile();
        }
    }
}

//public void OnTriggerEnter2D(Collider2D other)
//{
//    Debug.Log("Enemy Projectile hit SOMETHING");
//    // Check if the other object is an enemy
//    if (other.CompareTag("Player"))
//    {
//        Debug.Log("Enemey Projectile hit Player");
//        // Add logic to damage the player, someone please check this over I'm not too familiar
//        Character player = other.GetComponent<Character>();
//        if (player != null)
//        {
//            Debug.Log("Applying " + damage + " damage");
//            player.ApplyDamage(damage); // Example damage value
//        }

//        // Destroy the projectile after impact
//        Destroy(gameObject);
//    }
//}

