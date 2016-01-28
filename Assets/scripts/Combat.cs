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

    [HideInInspector]
    [SyncVar]
    public int numBullets = 0;
    public int maxBullets = 5;

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

	void Fire(GameObject player, Vector3 position, Vector3 turretRotation) {
		var bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(turretRotation)) as GameObject;

		// set direction of bullet and rotation
		bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * shotSpeed;
		bullet.GetComponent<Bullet>().player = this;
	}

    [Command]
	public void CmdFire(GameObject player, Vector3 position, Vector3 turretRotation) // turretRoration is passed in otherwise server's rotation is used
    {
        if (numBullets >= maxBullets)
        {
            return;
        }
        numBullets++;
		Fire (player, position, turretRotation);
		RpcFire (player, position, turretRotation);
    }

	[ClientRpc]
	public void RpcFire(GameObject player, Vector3 position, Vector3 turretRotation) {
		if (!isServer) {
			Fire (player, position, turretRotation);
		}
	}
}
