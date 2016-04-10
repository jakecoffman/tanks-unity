using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Pod : NetworkBehaviour {
    public const int maxHealth = 2;
    public int health = maxHealth;

    [Server]
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }
   
}
