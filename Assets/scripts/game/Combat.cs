using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Combat : NetworkBehaviour {

    public GameObject bulletPrefab;
    public GameObject tankExplosionPrefab;

    const float shotSpeed = 10f;
    const int maxHealth = 1;

    [SyncVar]
    public int health = maxHealth;
    [SyncVar]
    public bool isDead = false;

    [HideInInspector]
    [SyncVar]
    public int firedBullets = 0;
    public int maxBullets = 5;

    float _shotCooldown = 0.2f;
    float _shotTimer = 0;

    ParticleSystem _explosionParticles;
    AudioSource _explosionAudio;

    void Awake()
    {
        // Instantiate the explosion prefab and get a reference to the particle system on it.
        _explosionParticles = Instantiate(tankExplosionPrefab).GetComponent<ParticleSystem>();

        // Get a reference to the audio source on the instantiated prefab.
        _explosionAudio = _explosionParticles.GetComponent<AudioSource>();

        // Disable the prefab so it can be activated when it's required.
        _explosionParticles.gameObject.SetActive(false);
    }

    [Server]
    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0 && !isDead)
        {
            health = 0;
            isDead = true;
            RpcDie();

            GameObject gameManager = GameObject.Find("GameManager");
            gameManager.GetComponent<GameManager>().RemoveTank(gameObject);
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if (!isServer)
        {
            GameObject gameManager = GameObject.Find("GameManager");
            gameManager.GetComponent<GameManager>().RemoveTank(gameObject);
        }
        
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = Color.black;
        }
        // Move the instantiated explosion prefab to the tank's position and turn it on.
        var pos = transform.position;
        pos.y += 5;
        _explosionParticles.transform.position = pos;
        _explosionParticles.gameObject.SetActive(true);

        // Play the particle system of the tank exploding.
        _explosionParticles.Play();

        // Play the tank explosion sound effect.
        _explosionAudio.Play();
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
        RpcFired(bullet, player);
    }

    [ClientRpc]
    public void RpcFired(GameObject bullet, GameObject player)
    {
        if (isServer)
        {
            return;
        }
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * shotSpeed;
    }
}
