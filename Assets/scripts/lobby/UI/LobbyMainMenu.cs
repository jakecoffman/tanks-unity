using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//Main menu, mainly only a bunch of callback called by the UI (setup through the Inspector)
public class LobbyMainMenu : MonoBehaviour 
{
    public LobbyManager lobbyManager;

    public RectTransform lobbyServerList;
    public RectTransform lobbyPanel;

    public InputField ipInput;
    public InputField matchNameInput;

    public void OnEnable()
    {
        lobbyManager.topPanel.ToggleVisibility(true);

        ipInput.onEndEdit.RemoveAllListeners();
        ipInput.onEndEdit.AddListener(onEndEditIP);

        matchNameInput.onEndEdit.RemoveAllListeners();
        matchNameInput.onEndEdit.AddListener(onEndEditGameName);
    }

    // PLAY AND HOST
    public void OnClickHost()
    {
        lobbyManager.StartHost();
    }

    // LOCAL JOIN
    public void OnClickJoin()
    {
        lobbyManager.ChangeTo(lobbyPanel);

        lobbyManager.networkAddress = ipInput.text;
        lobbyManager.StartClient();

        lobbyManager.backDelegate = lobbyManager.StopClientClbk;
        lobbyManager.DisplayIsConnecting();

        lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
    }

    // DEDICATED SERVER (this should be removed, use batchmode)
    public void OnClickDedicated()
    {
        lobbyManager.ChangeTo(null);
        lobbyManager.StartServer();

        lobbyManager.backDelegate = lobbyManager.StopServerClbk;

        lobbyManager.SetServerInfo("Dedicated Server", lobbyManager.networkAddress);
    }

    // CREATE (online)
    public void OnClickCreateMatchmakingGame()
    {
        lobbyManager.StartMatchMaker();
        lobbyManager.matchMaker.CreateMatch(
            matchNameInput.text,
            lobbyManager.maxPlayers,
            true, // match advertise
            "", "", "", 0, 0,
            lobbyManager.OnMatchCreate); // creation callback

        lobbyManager.backDelegate = lobbyManager.StopHost;
        lobbyManager._isMatchmaking = true;
        lobbyManager.DisplayIsConnecting();

        lobbyManager.SetServerInfo("Matchmaker Host", lobbyManager.matchHost);
    }

    // LIST SERVERS
    public void OnClickOpenServerList()
    {
        lobbyManager.StartMatchMaker();
        lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
        lobbyManager.ChangeTo(lobbyServerList);
    }

    void onEndEditIP(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClickJoin();
        }
    }

    void onEndEditGameName(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClickCreateMatchmakingGame();
        }
    }

}
