using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AttachCamera : NetworkBehaviour {

    public override void OnStartLocalPlayer()
    {
        transform.Find("360 LOS Source High Precision").gameObject.SetActive(true);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>().m_Target = transform;
    }
}
