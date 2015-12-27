﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Bullet : NetworkBehaviour {

    [HideInInspector]
    public PlayerMovement player;

    void Start()
    {
        StartCoroutine(Destroy());
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }

    public override void OnNetworkDestroy()
    {
        Destroy(gameObject);
    }

    [ServerCallback]
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!isServer)
        {
            return;
        }
        var hit = collider.gameObject;
        if (hit.tag == "Player")
        {
            var combat = hit.GetComponent<Combat>();
            combat.TakeDamage(10);
            player.numBullets--;
            NetworkServer.Destroy(gameObject);
        }
        else if (hit.tag == "Wall")
        {
            Debug.Log("Wall hit");
            player.numBullets--;
            NetworkServer.Destroy(gameObject);
        }
    }
}
