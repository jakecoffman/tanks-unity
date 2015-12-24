using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerMovement : NetworkBehaviour {

    public float speed = 20f;
    public float turnSpeed = 3.5f;
    public float shotSpeed = 10f;
    public int maxBullets = 3;
    public GameObject bulletPrefab;

    private int currentBullets = 0;
    private bool isFiring = false;
    private float rotation = 0f;
    private Transform turret;

    public override void OnStartLocalPlayer()
    {
        turret.gameObject.GetComponent<Turret>().isLocalPlayer = isLocalPlayer;
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
        turret = transform.GetChild(1);
    }

    // called each frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        Move();
        if (Input.GetMouseButton(0))
        {
            CmdFire();
        }
    }
    
    // called in constant intervals
    void FixedUpdate()
    {
        
    }

    [Command]
    void CmdFire()
    {
        if (isFiring)
        {
            return;
        }
        isFiring = true;
        // place bullet
        var bullet = Instantiate(bulletPrefab, turret.position + turret.up * 0.9f, Quaternion.identity) as GameObject;

        // set direction of bullet and rotation
        bullet.transform.rotation = turret.rotation;
        bullet.GetComponent<Rigidbody2D>().velocity = turret.up * shotSpeed;

        NetworkServer.Spawn(bullet);
        Destroy(bullet, 2.0f);
        isFiring = false;
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
        
        //GetComponent<Rigidbody2D>().angularVelocity = 0;
        //float forwardInput = Input.GetAxis("Vertical");
        GetComponent<Rigidbody2D>().AddForce(gameObject.transform.up * move);
    }
}
