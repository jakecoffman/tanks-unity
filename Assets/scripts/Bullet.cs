using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D collider)
    {
        var hit = collider.gameObject;
        if (hit.tag == "Player")
        {
            //Destroy(gameObject);
        }
        else if (hit.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
