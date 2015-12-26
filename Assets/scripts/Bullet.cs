using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

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
            //Destroy(gameObject);
        }
        else if (hit.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
