using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerMovement : NetworkBehaviour {

    public float speed = 20f;
    public float turnSpeed = 3.5f;
    public float shotSpeed = 10f;
    public int maxBullets = 5;
    public GameObject bulletPrefab;

    // TODO: Bullet manager
    [HideInInspector]
    [SyncVar]
    public int numBullets = 0;

    private bool isFiring = false;
    private float rotation = 0f;
    private Transform turret;

    // Components
    private Combat combat;

    public override void OnStartLocalPlayer()
    {
        turret.gameObject.GetComponent<Turret>().player = GetComponent<PlayerMovement>();
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
        {
            r.material.color = Color.red;
        }
    }

    public override void OnStartServer()
    {
        
    }

    void Awake()
    {
        combat = GetComponent<Combat>();
        turret = transform.GetChild(1);
    }

    // called each frame
    void Update()
    {
        if (!isLocalPlayer || combat.isDead)
        {
            return;
        }
        Move();
        if (Input.GetMouseButton(0))
        {
            StartCoroutine(Fire());
        }
    }
    
    IEnumerator Fire()
    {
        if (isFiring || numBullets >= maxBullets)
        {
            yield break;
        }
        isFiring = true;
        numBullets++;
        CmdFire(gameObject);
        yield return new WaitForSeconds(0.1f);
        isFiring = false;
    }

    [Command]
    void CmdFire(GameObject player)
    {
        var bullet = Instantiate(bulletPrefab, turret.position + turret.up * 0.9f, Quaternion.identity) as GameObject;

        // set direction of bullet and rotation
        bullet.transform.rotation = turret.rotation;
        bullet.GetComponent<Rigidbody2D>().velocity = turret.up * shotSpeed;
        bullet.GetComponent<Bullet>().player = player.GetComponent<PlayerMovement>();

        NetworkServer.Spawn(bullet);
    }

    void Move()
    {
        float move = 0;
        if(Input.GetKey(KeyCode.W))
        {
            move = speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            move = -speed;
        }

        rotation = 0;
        if (Input.GetKey(KeyCode.A))
        {
            rotation = -turnSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            rotation = turnSpeed;
        }
        if (speed > 0)
        {
            rotation *= -1;
        }
        transform.Rotate(new Vector3(0, 0, rotation));
        
        GetComponent<Rigidbody2D>().AddForce(gameObject.transform.up * move);
    }
}
