using UnityEngine;
using System.Collections;
using LOS.Event;

public class TankVisibility : MonoBehaviour {

    Renderer _renderer;
    Renderer _childrender;

    void Start()
    {
        if (GetComponent<Tank>().isLocalPlayer)
        {
            return;
        }
        _renderer = GetComponent<Renderer>();
        _childrender = transform.GetChild(1).GetComponent<Renderer>();
        LOSEventTrigger trigger = GetComponent<LOSEventTrigger>();
        trigger.OnNotTriggered += OnNotLit;
        trigger.OnTriggered += OnLit;

        OnNotLit();
    }

    private void OnNotLit()
    {
        _renderer.enabled = false;
        _childrender.enabled = false;
    }

    private void OnLit()
    {
        _renderer.enabled = true;
        _childrender.enabled = true;
    }
}
