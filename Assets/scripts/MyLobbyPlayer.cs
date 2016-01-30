using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MyLobbyPlayer : NetworkLobbyPlayer {
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}
