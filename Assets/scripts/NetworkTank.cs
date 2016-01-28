using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkTank : NetworkBehaviour {
    public string playerName;
    public Color color;
    public float speed = 20f;
    public float turnSpeed = 3.5f;

	// TODO enforce server side
	public float timeBetweenShots = 0.2f;
    GameObject turret;

    private bool isFiring = false;

    private Combat combat;
    Rigidbody2D _rigid;

    public override void OnStartLocalPlayer()
    {
        foreach (SpriteRenderer r in GetComponentsInChildren<SpriteRenderer>())
        {
            r.material.color = Color.red;
        }
        _rigid = GetComponent<Rigidbody2D>();
    }

    void Awake()
    {
        turret = transform.GetChild(0).gameObject;
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

        if (combat.numBullets >= combat.maxBullets)
        {
            yield break;
        }
        isFiring = true;

		combat.CmdFire(gameObject, turret.transform.position + turret.transform.up * 0.6f, turret.transform.rotation.eulerAngles);
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

        float rotation = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            rotation += -turnSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            rotation += turnSpeed;
        }
        if (speed > 0)
        {
            rotation *= -1;
        }

        _rigid.MoveRotation(_rigid.rotation + rotation);
        _rigid.AddForce(transform.up * move);
    }
}
