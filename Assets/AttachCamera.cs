using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AttachCamera : NetworkBehaviour {

    void Start()
    {
        if (isLocalPlayer)
        {
            transform.Find("360 LOS Source High Precision").gameObject.SetActive(true);
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>().m_Target = transform;
            Transform model = transform.Find("Model");
            model.GetComponent<LOS.LOSCuller>().enabled = false;
            model.GetComponent<LOS.LOSObjectHider>().enabled = false;
        }
    }
}
