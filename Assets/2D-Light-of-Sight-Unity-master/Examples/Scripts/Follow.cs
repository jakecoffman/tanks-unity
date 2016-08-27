using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {

	public Transform targetTrans;

	void Update () {
        if (targetTrans == null)
        {
            return;
        }

        var pos = targetTrans.position;
        pos.z = transform.position.z;
		transform.position = pos;		
	}
}
