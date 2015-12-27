﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerMovement : NetworkBehaviour {

    public float speed = 20f;
    public float turnSpeed = 3.5f;
    public int maxBullets = 5;
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
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var rot = Quaternion.LookRotation(turret.transform.position - mousePos, Vector3.forward);
        turret.transform.rotation = rot;
        turret.transform.eulerAngles = new Vector3(0, 0, turret.transform.eulerAngles.z);
    }
    
    IEnumerator Fire()
    {
        if (isFiring)
        {
            Debug.Log("Already firing");
            yield break;
        }

        if (numBullets >= maxBullets)
        {
            Debug.Log("Out of bullets");
            yield break;
        }
        Debug.Log("Firing");
        isFiring = true;
        numBullets++;
        combat.CmdFire(gameObject);
        // TODO: Enforce server side
        yield return new WaitForSeconds(0.1f);
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
