using UnityEngine;
using UnityEngine.U2D;
using Unity.VisualScripting;
using UnityEngine.UI;

public class Boss324 : Enemy
{
    protected GameObject player;
    protected SpriteRenderer sprite;
    protected EnemyProjectile projectile;

    public Slider healthBar;
    // for ranged
    float attackRate = 1.0f;  // Time between attacks
    private float timeBtwShots;
    protected Vector2 kiteDistance = new Vector2(5f, 6f); // stay within these bounds

    protected override void setType()
    {
        this.type = AIType.Custom;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar.value = health;
        setType();
    }

    // Update is called once per frame
    void Update()
    {
        var playerPos = player.transform.position;
        var maxDistance = kiteDistance.x;
        var stoppingDistance = kiteDistance.y;

        // If the player is dead, stop shooting and go in a random direction
        if (player.GetComponent<Character>().currentHealth <= 0)
        {
            float random = Random.Range(0f, 2 * Mathf.PI);
            Vector2 randomVec = new Vector2(Mathf.Cos(random) * 100, Mathf.Sin(random) * 100);
            transform.position = Vector2.MoveTowards(transform.position, randomVec, (maxSpeed / 4f) * Time.deltaTime);
            return;
        }

        if (Vector2.Distance(transform.position, playerPos) > stoppingDistance)
        {
            transform.position = Vector2.Lerp(transform.position, Vector2.MoveTowards(transform.position, playerPos, maxSpeed * Time.deltaTime), acceleration);
        }
        else if (Vector2.Distance(transform.position, playerPos) < stoppingDistance && Vector2.Distance(transform.position, playerPos) > maxDistance)
        {
            // don't move
        }
        else if (Vector2.Distance(transform.position, playerPos) < maxDistance)
        {
            transform.position = Vector2.Lerp(transform.position, Vector2.MoveTowards(transform.position, playerPos, maxSpeed * Time.deltaTime * (-1)), acceleration);
        }


        if (timeBtwShots <= 0)
        {
            var projectileInstance = Instantiate(projectile, transform.position, Quaternion.identity);
            projectileInstance.damage = this.damage;
            timeBtwShots = attackRate;
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }
    }
}
