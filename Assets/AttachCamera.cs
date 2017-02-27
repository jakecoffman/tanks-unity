using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AttachCamera : NetworkBehaviour {

    public bool debug = false;

    void Start()
    {
        if (debug || isLocalPlayer)
        {
            CameraController cc = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            cc.m_Target = transform;
        } else { 
            transform.Find("360 LOS Source High Precision").gameObject.SetActive(false);
            Transform model = transform.Find("Model");
            model.GetComponent<LOS.LOSVisibilityInfo>().enabled = false;
            model.GetComponent<LOS.LOSStencilRenderer>().enabled = false;
            model.GetComponent<LOS.LOSCuller>().enabled = true;
            model.GetComponent<LOS.LOSObjectHider>().enabled = true;
        }
    }
}
