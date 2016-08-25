using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class VisionChecker : NetworkBehaviour {
    int visRange = 1000;
    float visUpdateInterval = 0.1f; // in seconds
    public bool forceHidden = false; // could use for stealth or something

    float m_VisUpdateTime;

    void Update()
    {
        if (!NetworkServer.active)
            return;

        if (Time.time - m_VisUpdateTime > visUpdateInterval)
        {
            GetComponent<NetworkIdentity>().RebuildObservers(false);
            m_VisUpdateTime = Time.time;
        }
    }

    // The OnCheckObservers function is called on the server on each networked object when a new player enters the game.
    // If it returns true, then that player is added to the object�s observers.
    public override bool OnCheckObserver(NetworkConnection newObserver)
    {
        if (forceHidden)
            return false;

        // this cant use newObserver.playerControllers[0]. must iterate to find a valid player.
        GameObject player = null;
        for (int i = 0; i < newObserver.playerControllers.Count; i++)
        {
            var p = newObserver.playerControllers[i];
            if (p != null && p.gameObject != null)
            {
                player = p.gameObject;
                break;
            }
        }
        if (player == null)
            return false;

        var pos = player.transform.position;
        return (pos - transform.position).magnitude < visRange;
    }

    // The OnRebuildObservers function is called on the server when RebuildObservers is invoked.
    // This function expects the set of observers to be populated with the players that can see the object.
    // The NetworkServer then handles sending ObjectHide and ObjectSpawn messages based on the differences between the old and new visibility sets.
    public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
    {
        Debug.Log("OnRebuildObservers");
        
        // ensure player can still see themself
        var uv = GetComponent<NetworkIdentity>();
        if (uv.connectionToClient != null)
        {
            observers.Add(uv.connectionToClient);
        }

        if (forceHidden)
        {
            return true;
        }

        // find players within range
        var hits = Physics2D.OverlapCircleAll(transform.position, visRange);
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            // (if an object has a connectionToClient, it is a player)
            uv = hit.GetComponent<NetworkIdentity>();
            if (uv != null && uv.connectionToClient != null)
            {
                var pos = uv.transform.position;
                var heading = pos - transform.position;
                var distance = heading.magnitude;
                var layers = 1 << LayerMask.NameToLayer("BlockingLayer") | 1 << LayerMask.NameToLayer("PodLayer");
                var ray = Physics2D.Raycast(transform.position, heading / distance, distance, layers);
                if (ray.collider == null)
                {
                    observers.Add(uv.connectionToClient);
                }
            }
        }
        return true;
    }

    // called hiding and showing objects on the host
    public override void OnSetLocalVisibility(bool vis)
    {
        SetVis(gameObject, vis);
    }

    static void SetVis(GameObject go, bool vis)
    {
        foreach (var r in go.GetComponents<Renderer>())
        {
            r.enabled = vis;
        }
        for (int i = 0; i < go.transform.childCount; i++)
        {
            var t = go.transform.GetChild(i);
            SetVis(t.gameObject, vis);
        }
    }
}
