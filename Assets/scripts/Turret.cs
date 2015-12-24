using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Turret : MonoBehaviour {

    // set by parent script
    [HideInInspector]
    public bool isLocalPlayer;

	// Update is called once per frame
	void FixedUpdate () {
	    if (!isLocalPlayer)
        {
            return;
        }
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var rot = Quaternion.LookRotation(transform.position - mousePos, Vector3.forward);
        transform.rotation = rot;
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
    }
}
