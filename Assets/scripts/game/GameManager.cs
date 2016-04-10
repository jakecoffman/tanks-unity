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
    
    void Start()
    {
        pText = playersText.GetComponent<Text>();
        cText = centerText.GetComponent<Text>();
        alivePlayers = GameObject.FindGameObjectsWithTag("Player");
    }

    void OnEnable()
    {
        Combat.EventTankDied += RemoveTank;
    }

    void OnDisable()
    {
        Combat.EventTankDied -= RemoveTank;
    }

    void RemoveTank(GameObject tank)
    {
        Debug.Log(tank.GetComponent<Tank>().playerName + " died");
        var aliveList = new List<GameObject>(alivePlayers);
        aliveList.Remove(tank);
        alivePlayers = aliveList.ToArray();
        playerDied = true;
        if (alivePlayers.Length <= 1)
        {
            StartCoroutine(ReturnToLobby());
        }
    }

    IEnumerator ReturnToLobby()
    {
        yield return new WaitForSeconds(secondsUntilBackToLobby);
        var lobby = NetworkManager.singleton as MyNetworkLobbyManager;
        lobby.ServerChangeScene(lobby.lobbyScene);
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
