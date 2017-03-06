using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Bullet : NetworkBehaviour {

    [HideInInspector]
    public Combat combat;
    public int damageGiven = 1;

	Rigidbody _rigid;
	RaycastHit rayHit;

	int bounce = 0;

    void Start()
    {
        _rigid = GetComponent<Rigidbody>();
        if (isServer)
        {
            Predestine();
        }
        StartCoroutine(SpendAfter());
    }

    IEnumerator SpendAfter()
    {
        yield return new WaitForSeconds(6f);
        SpendBullet();
    }

    // the physics is unreliable so occationally (like at start or after bounce) we recalculate the wall we expect to 
    // hit so if we hit between walls we can ignore the wall that we shouldn't have hit!
    void Predestine() {
        var ray = new Ray(transform.position, _rigid.velocity);
        var wasHit = Physics.Raycast(ray, out rayHit, 100f, 1 << LayerMask.NameToLayer("BlockingLayer"));
        if (!wasHit)
        {
            Debug.Log("Raycast hit nothing. Out of bounds?" + transform.position);
            SpendBullet();
        }
	}

	void SpendBullet() {
		combat.firedBullets--;
		NetworkServer.Destroy(gameObject);
	}

	[ServerCallback]
    void OnTriggerEnter(Collider collider)
    {
        var hit = collider.gameObject;
        if (hit.tag == "Player")
        {
            if (hit == this.combat.gameObject && bounce == 0)
            {
                // can't hit self before first bounce
                return;
            }
            SpendBullet();
            var combat = hit.GetComponent<Combat>();
            combat.TakeDamage(damageGiven);
        }
        else if (hit.tag == "Wall")
        {
            if (rayHit.collider == null)
            {
                // prevents firing through walls
                SpendBullet();
                return;
            }
            if (hit != rayHit.collider.gameObject) {
                // prevents hitting between walls that are touching
                return;
			}
			if (bounce < 1) {
                bounce++;

				var vel = Vector3.Reflect(_rigid.velocity, rayHit.normal);
                vel.y = 0;
                _rigid.velocity = vel;
                transform.rotation = Quaternion.LookRotation(_rigid.velocity);
				Predestine();

                RpcBounced(_rigid.velocity, transform.rotation);

				return;
			}
			SpendBullet ();
        }
        else if (hit.tag == "Bullet")
        {
			SpendBullet ();
        }
        else if (hit.name == "Pod")
        {
            hit.GetComponent<Pod>().TakeDamage(damageGiven);
			SpendBullet ();
        }
    }

    [ClientRpc]
    void RpcBounced(Vector2 velocity, Quaternion rotation)
    {
        if (!isServer)
        {
            _rigid.velocity = velocity;
            transform.rotation = rotation;
        }
    }
}
