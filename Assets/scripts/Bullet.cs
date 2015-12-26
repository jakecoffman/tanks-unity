using UnityEngine;
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
        Destroy(this);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        var hit = collider.gameObject;
        if (hit.tag == "Player")
        {
            var combat = hit.GetComponent<Combat>();
            combat.TakeDamage(10);
            Destroy(gameObject);
            if (isServer)
            {
                player.numBullets--;
            }
        }
        else if (hit.tag == "Wall")
        {
            Destroy(gameObject);
            if (isServer)
            {
                player.numBullets--;
            }
        }
    }
}
