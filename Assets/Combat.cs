﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Combat : NetworkBehaviour {

    public GameObject bulletPrefab;
    public const float shotSpeed = 10f;
    public const int maxHealth = 100;
    [SyncVar]
    public int health = maxHealth;
    [SyncVar]
    public bool isDead = false;

    [Server]
    public void TakeDamage(int amount)
    {
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

    [Command]
    public void CmdFire(GameObject player)
    {
        Transform turret = player.transform.GetChild(0);
        var bullet = Instantiate(bulletPrefab, turret.position + turret.up * 0.9f, Quaternion.identity) as GameObject;

        // set direction of bullet and rotation
        bullet.transform.rotation = turret.rotation;
        bullet.GetComponent<Rigidbody2D>().velocity = turret.up * shotSpeed;
        bullet.GetComponent<Bullet>().player = player.GetComponent<PlayerMovement>();

        NetworkServer.Spawn(bullet);
    }
}
