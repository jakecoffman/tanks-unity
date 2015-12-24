using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D collider)
    {
        Destroy(gameObject);
        var hit = collider.gameObject;
        if (hit.tag == "Player")
        {
            Destroy(gameObject);
        }
        else if (hit.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        print("Collision");
        Destroy(gameObject);
    }
}
