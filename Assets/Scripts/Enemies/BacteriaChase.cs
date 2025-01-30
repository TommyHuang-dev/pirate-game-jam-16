using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacteriaChase : Enemy
{
    protected override void setType()
    {
        this.type = AIType.BasicMelee;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other object is an enemy
        if (other.CompareTag("Player"))
        {
            // Damage the player on hit
            Character player = other.GetComponent<Character>();
            if (player != null)
            {
                Debug.Log("Applying " + damage + " damage");
                player.ApplyDamage(damage);
            }
        }
    }
}
