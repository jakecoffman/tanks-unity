using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerMovement : NetworkBehaviour {

    public float speed = 20f;
    public float turnSpeed = 3.5f;
    public int maxBullets = 5;

	// TODO enforce server side
	public float timeBetweenShots = 0.2f;
    public GameObject turret;

    [HideInInspector]
    [SyncVar]
    public int numBullets = 0;

    private bool isFiring = false;
    private float rotation = 0f;

    private Combat combat;

    public override void OnStartLocalPlayer()
    {
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
        {
            r.material.color = Color.red;
        }
    }

    void Awake()
    {
        combat = GetComponent<Combat>();
    }

    // called each frame
    void FixedUpdate()
    {
        if (!isLocalPlayer || combat.isDead)
        {
            return;
        }
        Move();
        Aim();
        if (Input.GetMouseButton(0))
        {
            StartCoroutine(Fire());
        }
    }

    void Aim()
    {
        Vector3 mouse_pos = Input.mousePosition;
        mouse_pos.z = 0.0f;
        Vector3 object_pos = Camera.main.WorldToScreenPoint(transform.position);
        mouse_pos.x = mouse_pos.x - object_pos.x;
        mouse_pos.y = mouse_pos.y - object_pos.y;
        // -90 because my sprite is aiming up
        float angle = (Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg) - 90;
        Vector3 rotationVector = new Vector3(0, 0, angle);
        turret.transform.rotation = Quaternion.Euler(rotationVector);
    }

    IEnumerator Fire()
    {
        if (isFiring)
        {
            yield break;
        }

        if (numBullets >= maxBullets)
        {
            yield break;
        }
        isFiring = true;
        numBullets++;

        combat.CmdFire(gameObject, turret.transform.rotation.eulerAngles);
        // TODO: Enforce server side?
        yield return new WaitForSeconds(timeBetweenShots);
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
        GetComponent<Rigidbody2D>().AddForce(gameObject.transform.up * move);
    }
}
