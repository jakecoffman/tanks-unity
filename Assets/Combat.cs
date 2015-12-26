using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Combat : NetworkBehaviour {

    public const int maxHealth = 100;
    [SyncVar]
    public int health = maxHealth;
    [SyncVar]
    public bool isDead = false;

    public void TakeDamage(int amount)
    {
        if (!isServer)
        {
            return;
        }
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            isDead = true;
            foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
            {
                r.material.color = Color.black;
            }
        }
    }
}
