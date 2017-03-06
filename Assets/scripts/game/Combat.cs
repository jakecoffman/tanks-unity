using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Combat : NetworkBehaviour {

    public GameObject bulletPrefab;

    const float shotSpeed = 10f;
    const int maxHealth = 1;

    [SyncVar]
    public int health = maxHealth;
    [SyncVar]
    public bool isDead = false;

	GameObject smoke;

    [HideInInspector]
    [SyncVar]
    public int firedBullets = 0;
    public int maxBullets = 5;

    float _shotCooldown = 0.2f;
    float _shotTimer = 0;

    [Server]
    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0 && !isDead)
        {
            health = 0;
            isDead = true;
            RpcDie();
            smoke = Instantiate(GetComponent<Tank>().smokePrefab, transform.position, Quaternion.identity) as GameObject;
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

    void Update()
    {
        _shotTimer += Time.deltaTime;
    }

    public IEnumerator Fire(Transform barrel)
    {
        if (firedBullets >= maxBullets)
        {
            yield break;
        }

        CmdFire(gameObject, barrel.position, barrel.rotation);
        yield return new WaitForSeconds(_shotCooldown);
    }

    [Command]
	public void CmdFire(GameObject player, Vector3 position, Quaternion turretRotation) // turretRoration is passed in otherwise server's rotation is used
    {
        if (_shotTimer < _shotCooldown)
        {
            return;
        }
        if (firedBullets >= maxBullets)
        {
            return;
        }
        _shotTimer = 0;
        firedBullets++;
        var bullet = Instantiate(bulletPrefab, position, turretRotation) as GameObject;

        // set direction of bullet and rotation
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * shotSpeed;
        bullet.GetComponent<Bullet>().combat = this;
        bullet.GetComponent<Renderer>().materials[0].color = player.transform.Find("Model").Find("TankChassis").GetComponent<Renderer>().materials[0].color;
        NetworkServer.Spawn (bullet);
        //RpcFired(bullet, player);
    }
}
