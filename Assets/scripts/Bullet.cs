using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Bullet : NetworkBehaviour {

    [HideInInspector]
    public PlayerMovement player;
    public int damageGiven = 1;

	Rigidbody2D rigid2d;
	RaycastHit2D rayHit;

	int bounce = 0;

    void Start()
    {
		rigid2d = GetComponent<Rigidbody2D>();
		Predestine ();
    }

	// the physics is unreliable so occationally (like at start or after bounce) we recalculate the wall we expect to 
	// hit so if we hit between walls we can ignore the wall that we shouldn't have hit!
	void Predestine() {
		rayHit = Physics2D.Raycast(transform.position, rigid2d.velocity, 100f, 1 << LayerMask.NameToLayer("BlockingLayer"));
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
            // TODO: for bouncy bullets, allow players to hit themselves
            if (hit == player.gameObject)
            {
                return;
            }
            var combat = hit.GetComponent<Combat>();
            combat.TakeDamage(damageGiven);
            player.numBullets--;
            NetworkServer.Destroy(gameObject);
        }
        else if (hit.tag == "Wall")
        {
			if (hit != rayHit.collider.gameObject) {
				// prevents hitting between walls that are touching
				return;
			}
			if (bounce < 1) {
				bounce++;

				rigid2d.velocity = Vector2.Reflect(rigid2d.velocity, rayHit.normal);
				Predestine();
				return;
			}
            player.numBullets--;
            NetworkServer.Destroy(gameObject);
        }
        else if (hit.tag == "Bullet")
        {
            player.numBullets--;
            NetworkServer.Destroy(gameObject);
        }
        else if (hit.name == "Pod")
        {
            player.numBullets--;
            hit.GetComponent<Pod>().TakeDamage(damageGiven);
            NetworkServer.Destroy(gameObject);
        }
    }
}
