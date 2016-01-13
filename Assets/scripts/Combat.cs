using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Combat : NetworkBehaviour {

    public GameObject bulletPrefab;
    public const float shotSpeed = 5f;
    public const int maxHealth = 1;
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
            RpcDie();
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
        {
            r.material.color = Color.black;
        }
    }

    [Command]
    public void CmdFire(GameObject player, Vector3 eulerAngles)
    {
        Transform turret = player.transform.GetChild(0);
        var bullet = Instantiate(bulletPrefab, turret.position + turret.up * 0.6f, Quaternion.identity) as GameObject;

        // set direction of bullet and rotation
        bullet.transform.eulerAngles = turret.eulerAngles;
        bullet.transform.eulerAngles = new Vector3(0, 0, bullet.transform.eulerAngles.z);
        bullet.GetComponent<Rigidbody2D>().velocity = turret.up * shotSpeed;
        bullet.GetComponent<Bullet>().player = player.GetComponent<PlayerMovement>();

        NetworkServer.Spawn(bullet);
    }
}
