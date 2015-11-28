using UnityEngine;
using System.Collections;

public class PlayerTurret : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var rot = Quaternion.LookRotation(transform.position - mousePos, Vector3.forward);
        transform.rotation = rot;
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
    }
}
