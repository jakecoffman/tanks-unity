using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour {

    public GameObject playersText;
    Text pText;

    GameObject[] alivePlayers;

    void Start()
    {
        pText = playersText.GetComponent<Text>();
    }

    public override void OnStartServer()
    {
        alivePlayers = GameObject.FindGameObjectsWithTag("Player");
        RpcAlivePlayers(alivePlayers);
    }

    void OnEnable()
    {
        Combat.OnTankDied += RemoveTank;
    }

    void OnDisable()
    {
        Combat.OnTankDied -= RemoveTank;
    }

    // Server side only
    void RemoveTank(GameObject tank)
    {
        Debug.Log(tank.GetComponent<Tank>().playerName + " died");
        var aliveList = new List<GameObject>(alivePlayers);
        aliveList.Remove(tank);
        alivePlayers = aliveList.ToArray();
        RpcAlivePlayers(alivePlayers);
        if (alivePlayers.Length == 0)
        {
            var lobby = NetworkManager.singleton as MyNetworkLobbyManager;
            if (lobby)
            {
                lobby.ServerChangeScene(lobby.lobbyScene);
            }
            else
            {
                Debug.Log("Can't get lobby manager!!!!");
            }
        }
    }

    [ClientRpc]
    void RpcAlivePlayers(GameObject[] alive)
    {
        alivePlayers = alive;
    }

    void OnGUI()
    {
        pText.text = "Players left: " + alivePlayers.Length;
    }

    void OnValidate()
    {
        if (playersText == null)
        {
            Debug.Log("Set players text");
        }
    }
}
