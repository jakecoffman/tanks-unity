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

	GameObject smoke;

    [HideInInspector]
    [SyncVar]
    public int firedBullets = 0;
    public int maxBullets = 5;

    [Server]
    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0 && !isDead)
        {
            health = 0;
            isDead = true;
            RpcDie();
            GameObject gameManager = GameObject.FindGameObjectWithTag("GameController");
            gameManager.GetComponent<GameManager>().RemoveTank(gameObject);
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if (!isServer)
        {
            GameObject gameManager = GameObject.FindGameObjectWithTag("GameController");
            gameManager.GetComponent<GameManager>().RemoveTank(gameObject);
        }
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
        {
            r.material.color = Color.gray;
        }
		smoke = Instantiate (GetComponent<Tank> ().smokePrefab, transform.position, Quaternion.identity) as GameObject;
    }

	void FixedUpdate() {
		if (smoke != null) {
			smoke.transform.position = transform.position;
		}
	}

    [Command]
	public void CmdFire(GameObject player, Vector3 position, Vector3 turretRotation) // turretRoration is passed in otherwise server's rotation is used
    {
        if (firedBullets >= maxBullets)
        {
            return;
        }
        firedBullets++;
        var bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(turretRotation)) as GameObject;

        // set direction of bullet and rotation
        bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * shotSpeed;
        bullet.GetComponent<Bullet>().combat = this;
        bullet.GetComponent<Renderer>().material.color = player.GetComponent<Tank>().color;
        NetworkServer.Spawn (bullet);
        RpcFired(bullet, player);
        Destroy(bullet, 10.0f);
    }

    [ClientRpc]
    void RpcFired(GameObject bullet, GameObject player)
    {
        bullet.GetComponent<Bullet>().combat = this;
        bullet.GetComponent<Renderer>().material.color = player.GetComponent<Tank>().color;
    }

}
