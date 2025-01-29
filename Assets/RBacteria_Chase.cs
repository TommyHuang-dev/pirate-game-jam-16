using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBacteria_Chase : MonoBehaviour
{
    public GameObject player;
    public float speed;
    public float distanceBetween;
    public int healthPoints = 10;

    private float distance;

    SpriteRenderer sprite;
    private float damageFlash = 0;  // set after taking damage, ticks down

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {   
        DirectionFacing();
        // set up for distance and direction bacteria need to move to
        distance = Vector2.Distance(transform.position, player.transform.position);
        Vector2 direction = player.transform.position - transform.position;

        // normalize the vector so that it points to the player
        //direction.Normalize();
        //float angle = Mathf.Atan2(direction.y direction.x) * Mathf.Rad2Deg;


        if (distance < distanceBetween)
        {
            // Calculate how to move to player and how fast
            transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, speed * Time.deltaTime);
            // Rotate
            //transform.rotation = Quaternion.Euler(Vector3.forward * angle);
        }

        sprite.color = new Color(1, 1 - damageFlash, 1 - damageFlash);
        damageFlash = Mathf.Max(0, damageFlash - 4 * Time.deltaTime);
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

    void DirectionFacing()
    {
        Vector2 directionToTarget = (player.transform.position - transform.position).normalized;
        float dotProduct = Vector2.Dot(transform.right, directionToTarget);

        if (dotProduct > 0)
        {
            sprite.flipX = true;
        }
        else if (dotProduct < 0)
        {
            sprite.flipX = false;
        }
    }
}
