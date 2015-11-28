using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public float speed = 60f;
    public float turnSpeed = 1.5f;

    private float rotation = 0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // called once per physics tick
    void FixedUpdate()
    {
        rotation += Input.GetAxis("Horizontal") * turnSpeed;
        if (speed > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, -rotation);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, rotation);
        }

        //GetComponent<Rigidbody2D>().angularVelocity = 0;
        float forwardInput = Input.GetAxis("Vertical");
        GetComponent<Rigidbody2D>().AddForce(gameObject.transform.up * speed * forwardInput);
    }
}
