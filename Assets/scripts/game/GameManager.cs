using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour {

    public GameObject playersText;
    public GameObject centerText;
    Text pText;
    Text cText;

    GameObject[] alivePlayers;
    bool playerDied;
    int secondsUntilBackToLobby = 5;

    void Awake()
    {
        pText = playersText.GetComponent<Text>();
        cText = centerText.GetComponent<Text>();
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
        if (alivePlayers.Length <= 1)
        {
            StartCoroutine(ReturnToLobby());
        }
    }

    IEnumerator ReturnToLobby()
    {
        yield return new WaitForSeconds(secondsUntilBackToLobby);
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

    [ClientRpc]
    void RpcAlivePlayers(GameObject[] alive)
    {
        alivePlayers = alive;
        playerDied = true;
    }

    void OnGUI()
    {
        pText.text = "Players left: " + alivePlayers.Length;
        if (playerDied)
        {
            playerDied = false;
            if (alivePlayers.Length == 1)
            {
                cText.text = alivePlayers[0].GetComponent<Tank>().playerName + " won!";
            }
            else if (alivePlayers.Length == 0)
            {
                cText.text = "Everyone died.";
            }
        }
    }

    void OnValidate()
    {
        if (playersText == null || centerText == null)
        {
            Debug.Log("Set text GameObjects in GameManager");
        }
    }
}
