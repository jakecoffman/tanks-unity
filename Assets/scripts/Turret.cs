using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Turret : MonoBehaviour {

    public PlayerMovement player;
    public Combat combat;

    void Awake()
    {
        player = GetComponentInParent<PlayerMovement>();
        combat = GetComponentInParent<Combat>();
    }

	void Update () {
        if (!player.isLocalPlayer || combat.isDead)
        {
            return;
        }
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var rot = Quaternion.LookRotation(transform.position - mousePos, Vector3.forward);
        transform.rotation = rot;
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
    }
}
