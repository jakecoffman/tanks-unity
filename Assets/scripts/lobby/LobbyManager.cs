using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : NetworkManager 
{
    static short MsgKicked = MsgType.Highest + 1;

    static public LobbyManager s_Singleton;

    float prematchCountdown = 5.0f;

    [Space]
    [Header("UI Reference")]
    public LobbyTopPanel topPanel;

    public RectTransform mainMenuPanel;
    public RectTransform lobbyPanel;

    public LobbyInfoPanel infoPanel;
    public LobbyCountdownPanel countdownPanel;
    public GameObject addPlayerButton;

    protected RectTransform currentPanel;

    public Button backButton;

    public Text statusInfo;
    public Text hostInfo;

    //Client numPlayers from NetworkManager is always 0, so we count (throught connect/destroy in LobbyPlayer) the number
    //of players, so that even client know how many player there is.
    [HideInInspector]
    public int _playerNumber = 0;

    //used to disconnect a client properly when exiting the matchmaker
    [HideInInspector]
    public bool _isMatchmaking = false;

    protected bool _disconnectServer = false;
    
    // used to stop matchmaking by telling which match to stop hosting
    protected ulong _currentMatchID;

    // **** these were variables from the old MyNetworkLobbyManager

    struct PendingPlayer
    {
        public NetworkConnection conn;
        public GameObject lobbyPlayer;
    }

    // configuration
    [SerializeField]
    bool m_ShowLobbyGUI = true;
    [SerializeField]
    public uint maxPlayers = 4;
    [SerializeField]
    uint minPlayers;
    [SerializeField]
    LobbyPlayer m_LobbyPlayerPrefab;
    [SerializeField]
    GameObject m_GamePlayerPrefab;
    [SerializeField]
    string m_LobbyScene = "";
    [SerializeField]
    string m_PlayScene = "";

    // runtime data
    List<PendingPlayer> m_PendingPlayers = new List<PendingPlayer>();
    public LobbyPlayer[] lobbySlots;

    // static message objects to avoid runtime-allocations
    static LobbyReadyToBeginMessage s_ReadyToBeginMessage = new LobbyReadyToBeginMessage();
    static IntegerMessage s_SceneLoadedMessage = new IntegerMessage();
    static LobbyReadyToBeginMessage s_LobbyReadyToBeginMessage = new LobbyReadyToBeginMessage();

    // properties
    public bool showLobbyGUI { get { return m_ShowLobbyGUI; } set { m_ShowLobbyGUI = value; } }
    public LobbyPlayer lobbyPlayerPrefab { get { return m_LobbyPlayerPrefab; } set { m_LobbyPlayerPrefab = value; } }
    public GameObject gamePlayerPrefab { get { return m_GamePlayerPrefab; } set { m_GamePlayerPrefab = value; } }
    public string lobbyScene { get { return m_LobbyScene; } set { m_LobbyScene = value; } }
    public string playScene { get { return m_PlayScene; } set { m_PlayScene = value; } }

    void Start()
    {
        s_Singleton = this;
        currentPanel = mainMenuPanel;

        backButton.gameObject.SetActive(false);
        GetComponent<Canvas>().enabled = true;

        DontDestroyOnLoad(gameObject);

        SetServerInfo("Offline", "None");
    }

    public void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name == lobbyScene)
        {
            if (topPanel.isInGame)
            {
                ChangeTo(lobbyPanel);
                if (_isMatchmaking)
                {
                    if (conn.playerControllers[0].unetView.isServer)
                    {
                        backDelegate = StopHostClbk;
                    }
                    else
                    {
                        backDelegate = StopClientClbk;
                    }
                }
                else
                {
                    if (conn.playerControllers[0].unetView.isClient)
                    {
                        backDelegate = StopHostClbk;
                    }
                    else
                    {
                        backDelegate = StopClientClbk;
                    }
                }
            }
            else
            {
                ChangeTo(mainMenuPanel);
            }

            topPanel.ToggleVisibility(true);
            topPanel.isInGame = false;
        }
        else
        {
            ChangeTo(null);

            Destroy(GameObject.Find("MainMenuUI(Clone)"));

            //backDelegate = StopGameClbk;
            topPanel.isInGame = true;
            topPanel.ToggleVisibility(false);
        }
    }

    public void ChangeTo(RectTransform newPanel)
    {
        if (currentPanel != null)
        {
            currentPanel.gameObject.SetActive(false);
        }

        if (newPanel != null)
        {
            newPanel.gameObject.SetActive(true);
        }

        currentPanel = newPanel;

        if (currentPanel != mainMenuPanel)
        {
            backButton.gameObject.SetActive(true);
        }
        else
        {
            backButton.gameObject.SetActive(false);
            SetServerInfo("Offline", "None");
            _isMatchmaking = false;
        }
    }

    // the connecting modal
    public void DisplayIsConnecting()
    {
        var _this = this;
        infoPanel.Display("Connecting...", "Cancel", () => { _this.backDelegate(); });
    }

    // the info at the top of the screen
    public void SetServerInfo(string status, string host)
    {
        statusInfo.text = status;
        hostInfo.text = host;
    }


    public delegate void BackButtonDelegate();
    // Loaded up with whatever contextual back would do
    public BackButtonDelegate backDelegate;
    
    // Called when the back button is hit
    public void GoBackButton()
    {
        backDelegate();
    }

    // ----------------- Server management

    public void AddLocalPlayer()
    {
        TryToAddPlayer();
    }

    public void RemovePlayer(LobbyPlayer player)
    {
        player.RemovePlayer();
    }

    public void SimpleBackClbk()
    {
        ChangeTo(mainMenuPanel);
    }
             
    public void StopHostClbk()
    {
        if (_isMatchmaking)
        {
            this.matchMaker.DestroyMatch((NetworkID)_currentMatchID, OnMatchDestroyed);
            _disconnectServer = true;
        }
        else
        {
            StopHost();
        }

        
        ChangeTo(mainMenuPanel);
    }

    public void StopClientClbk()
    {
        StopClient();

        if (_isMatchmaking)
        {
            StopMatchMaker();
        }

        ChangeTo(mainMenuPanel);
    }

    public void StopServerClbk()
    {
        StopServer();
        ChangeTo(mainMenuPanel);
    }

    class KickMsg : MessageBase { }
    public void KickPlayer(NetworkConnection conn)
    {
        conn.Send(MsgKicked, new KickMsg());
    }




    public void KickedMessageHandler(NetworkMessage netMsg)
    {
        infoPanel.Display("Kicked by Server", "Close", null);
        netMsg.conn.Disconnect();
    }

    //===================

    public override void OnStartHost()
    {
        base.OnStartHost();

        ChangeTo(lobbyPanel);
        backDelegate = StopHostClbk;
        SetServerInfo("Hosting", networkAddress);
    }

    public override void OnMatchCreate(CreateMatchResponse matchInfo)
    {
        base.OnMatchCreate(matchInfo);

        _currentMatchID = (System.UInt64)matchInfo.networkId;
    }

    public void OnMatchDestroyed(BasicResponse resp)
    {
        if (_disconnectServer)
        {
            StopMatchMaker();
            StopHost();
        }
    }

    //allow to handle the (+) button to add/remove player
    public void OnPlayersNumberModified(int count)
    {
        _playerNumber += count;

        // could be used to check max players per connection, but not sure why I care
        int localPlayerCount = 0;
        foreach (PlayerController p in ClientScene.localPlayers)
            localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;

        addPlayerButton.SetActive(_playerNumber < maxPlayers);
    }

    // ----------------- Server callbacks ------------------

    //we want to disable the button JOIN if we don't have enough player
    //But OnLobbyClientConnect isn't called on hosting player. So we override the lobbyPlayer creation
    public GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;

        LobbyPlayer newPlayer = obj.GetComponent<LobbyPlayer>();
        newPlayer.ToggleJoinButton(numPlayers + 1 >= minPlayers);


        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

            if (p != null)
            {
                p.RpcUpdateRemoveButton();
                p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
            }
        }

        return obj;
    }

    public void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
    {
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

            if (p != null)
            {
                p.RpcUpdateRemoveButton();
                p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
            }
        }
    }

    public void OnLobbyServerDisconnect(NetworkConnection conn)
    {
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

            if (p != null)
            {
                p.RpcUpdateRemoveButton();
                p.ToggleJoinButton(numPlayers >= minPlayers);
            }
        }

    }

    public bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        Tank tank = gamePlayer.GetComponent<Tank>();

        tank.playerName = lobby.playerName;
        tank.color = lobby.playerColor;
        
        return true;
    }

    // --- Countdown management

    public void OnLobbyServerPlayersReady()
    {
        StartCoroutine(ServerCountdownCoroutine());
    }

    public IEnumerator ServerCountdownCoroutine()
    {
        float remainingTime = prematchCountdown;
        int floorTime = Mathf.FloorToInt(remainingTime);

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.FloorToInt(remainingTime);

            if (newFloorTime != floorTime)
            {//to avoid flooding the network of message, we only send a notice to client when the number of plain seconds change.
                floorTime = newFloorTime;

                for (int i = 0; i < lobbySlots.Length; ++i)
                {
                    if (lobbySlots[i] != null)
                    {//there is maxPlayer slots, so some could be == null, need to test it before accessing!
                        (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(floorTime);
                    }
                }
            }
        }

        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            if (lobbySlots[i] != null)
            {
                (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(0);
            }
        }

        ServerChangeScene(playScene);
    }

    // ----------------- Client callbacks ------------------

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        infoPanel.gameObject.SetActive(false);

        conn.RegisterHandler(MsgKicked, KickedMessageHandler);

        if (!NetworkServer.active)
        {//only to do on pure client (not self hosting client)
            ChangeTo(lobbyPanel);
            backDelegate = StopClientClbk;
            SetServerInfo("Client", networkAddress);
        }
    }


    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        ChangeTo(mainMenuPanel);
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        ChangeTo(mainMenuPanel);
        infoPanel.Display("Cient error : " + (errorCode == 6 ? "timeout" : errorCode.ToString()), "Close", null);
    }

    // ------------------------ client handlers ------------------------

    public override void OnStartClient(NetworkClient lobbyClient)
    {
        if (lobbySlots.Length == 0)
        {
            lobbySlots = new LobbyPlayer[maxPlayers];
        }

        if (m_LobbyPlayerPrefab == null || m_LobbyPlayerPrefab.gameObject == null)
        {
            if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager no LobbyPlayer prefab is registered. Please add a LobbyPlayer prefab."); }
        }
        else
        {
            ClientScene.RegisterPrefab(m_LobbyPlayerPrefab.gameObject);
        }

        if (m_GamePlayerPrefab == null)
        {
            if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager no GamePlayer prefab is registered. Please add a GamePlayer prefab."); }
        }
        else
        {
            ClientScene.RegisterPrefab(m_GamePlayerPrefab);
        }

        lobbyClient.RegisterHandler(MsgType.LobbyReadyToBegin, OnClientReadyToBegin);
        lobbyClient.RegisterHandler(MsgType.LobbyAddPlayerFailed, OnClientAddPlayerFailedMessage);
    }

    public override void OnStopClient()
    {
        CallOnClientExitLobby();
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        string loadedSceneName = SceneManager.GetSceneAt(0).name;
        if (loadedSceneName == m_LobbyScene)
        {
            if (client.isConnected)
            {
                CallOnClientEnterLobby();
            }
        }
        else
        {
            CallOnClientExitLobby();
        }

        base.OnClientSceneChanged(conn);
        OnLobbyClientSceneChanged(conn);
    }

    void OnClientReadyToBegin(NetworkMessage netMsg)
    {
        netMsg.ReadMessage(s_LobbyReadyToBeginMessage);

        if (s_LobbyReadyToBeginMessage.slotId >= lobbySlots.Length)
        {
            if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnClientReadyToBegin invalid lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
            return;
        }

        var lobbyPlayer = lobbySlots[s_LobbyReadyToBeginMessage.slotId];
        if (lobbyPlayer == null || lobbyPlayer.gameObject == null)
        {
            if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager OnClientReadyToBegin no player at lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
            return;
        }

        lobbyPlayer.readyToBegin = s_LobbyReadyToBeginMessage.readyState;
        lobbyPlayer.OnClientReady(s_LobbyReadyToBeginMessage.readyState);
    }

    void OnClientAddPlayerFailedMessage(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager Add Player failed."); }
    }

    // UI

    void OnGUI()
    {
        if (!showLobbyGUI)
            return;

        string loadedSceneName = SceneManager.GetSceneAt(0).name;
        if (loadedSceneName != m_LobbyScene)
            return;

        Rect backgroundRec = new Rect(90, 180, 500, 150);
        GUI.Box(backgroundRec, "Players:");

        if (NetworkClient.active)
        {
            Rect addRec = new Rect(100, 300, 120, 20);
            if (GUI.Button(addRec, "Add Player"))
            {
                TryToAddPlayer();
            }
        }
    }

    public void TryToAddPlayer()
    {
        if (NetworkClient.active)
        {
            short controllerId = -1;
            var controllers = NetworkClient.allClients[0].connection.playerControllers;

            if (controllers.Count < maxPlayers)
            {
                controllerId = (short)controllers.Count;
            }
            else
            {
                for (short i = 0; i < maxPlayers; i++)
                {
                    if (!controllers[i].IsValid)
                    {
                        controllerId = i;
                        break;
                    }
                }
            }
            if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager TryToAddPlayer controllerId " + controllerId + " ready:" + ClientScene.ready); }

            if (controllerId == -1)
            {
                if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager No Space!"); }
                return;
            }

            if (ClientScene.ready)
            {
                ClientScene.AddPlayer(controllerId);
            }
            else
            {
                ClientScene.AddPlayer(NetworkClient.allClients[0].connection, controllerId);
            }
        }
        else
        {
            if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager NetworkClient not active!"); }
        }
    }

    // Unity editor calls this to make sure the inputs in the editor are correct
    void OnValidate()
    {
        if (minPlayers < 0)
        {
            minPlayers = 0;
        }

        if (minPlayers > maxPlayers)
        {
            minPlayers = maxPlayers;
        }

        if (m_LobbyPlayerPrefab != null)
        {
            var uv = m_LobbyPlayerPrefab.GetComponent<NetworkIdentity>();
            if (uv == null)
            {
                m_LobbyPlayerPrefab = null;
                Debug.LogWarning("LobbyPlayer prefab must have a NetworkIdentity component.");
            }
        }

        if (m_GamePlayerPrefab != null)
        {
            var uv = m_GamePlayerPrefab.GetComponent<NetworkIdentity>();
            if (uv == null)
            {
                m_GamePlayerPrefab = null;
                Debug.LogWarning("GamePlayer prefab must have a NetworkIdentity component.");
            }
        }
    }

    Byte FindSlot()
    {
        for (byte i = 0; i < maxPlayers; i++)
        {
            if (lobbySlots[i] == null)
            {
                return i;
            }
        }
        return Byte.MaxValue;
    }

    void SceneLoadedForPlayer(NetworkConnection conn, GameObject lobbyPlayerGameObject)
    {
        var lobbyPlayer = lobbyPlayerGameObject.GetComponent<LobbyPlayer>();
        if (lobbyPlayer == null)
        {
            // not a lobby player.. dont replace it
            return;
        }

        string loadedSceneName = SceneManager.GetSceneAt(0).name;
        if (LogFilter.logDebug) { Debug.Log("NetworkLobby SceneLoadedForPlayer scene:" + loadedSceneName + " " + conn); }

        if (loadedSceneName == m_LobbyScene)
        {
            // cant be ready in lobby, add to ready list
            PendingPlayer pending;
            pending.conn = conn;
            pending.lobbyPlayer = lobbyPlayerGameObject;
            m_PendingPlayers.Add(pending);
            return;
        }

        var controllerId = lobbyPlayerGameObject.GetComponent<NetworkIdentity>().playerControllerId;
        GameObject gamePlayer;

        // get start position from base class
        Transform startPos = GetStartPosition();
        if (startPos != null)
        {
            gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, startPos.position, startPos.rotation);
        }
        else
        {
            gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
        }

        if (!OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gamePlayer))
        {
            return;
        }

        // replace lobby player with game player
        NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, controllerId);
    }

    static int CheckConnectionIsReadyToBegin(NetworkConnection conn)
    {
        int countPlayers = 0;
        foreach (var player in conn.playerControllers)
        {
            if (player.IsValid)
            {
                var lobbyPlayer = player.gameObject.GetComponent<LobbyPlayer>();
                if (lobbyPlayer.readyToBegin)
                {
                    countPlayers += 1;
                }
            }
        }
        return countPlayers;
    }

    public void CheckReadyToBegin()
    {
        string loadedSceneName = SceneManager.GetSceneAt(0).name;
        if (loadedSceneName != m_LobbyScene)
        {
            return;
        }

        int totalPlayers = 0;
        int readyCount = 0;

        foreach (var conn in NetworkServer.connections)
        {
            if (conn == null)
                continue;
            totalPlayers += 1;
            readyCount += CheckConnectionIsReadyToBegin(conn);
        }
        if (readyCount < minPlayers || readyCount < totalPlayers)
        {
            // not enough players ready yet.
            return;
        }

        m_PendingPlayers.Clear();
        OnLobbyServerPlayersReady();
    }

    public void ServerReturnToLobby()
    {
        if (!NetworkServer.active)
        {
            Debug.Log("ServerReturnToLobby called on client");
            return;
        }
        ServerChangeScene(m_LobbyScene);
    }

    void CallOnClientEnterLobby()
    {
        foreach (var player in lobbySlots)
        {
            if (player == null)
                continue;

            player.readyToBegin = false;
            player.OnClientEnterLobby();
        }
    }

    void CallOnClientExitLobby()
    {
        foreach (var player in lobbySlots)
        {
            if (player == null)
                continue;

            player.OnClientExitLobby();
        }
    }

    public bool SendReturnToLobby()
    {
        if (client == null || !client.isConnected)
        {
            return false;
        }

        var msg = new EmptyMessage();
        client.Send(MsgType.LobbyReturnToLobby, msg);
        return true;
    }

    // ------------------------ server handlers ------------------------

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxPlayers)
        {
            conn.Disconnect();
            return;
        }

        // cannot join game in progress
        string loadedSceneName = SceneManager.GetSceneAt(0).name;
        if (loadedSceneName != m_LobbyScene)
        {
            conn.Disconnect();
            return;
        }

        base.OnServerConnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        // if lobbyplayer for this connection has not been destroyed by now, then destroy it here
        for (int i = 0; i < lobbySlots.Length; i++)
        {
            var player = lobbySlots[i];
            if (player == null)
                continue;

            if (player.connectionToClient == conn)
            {
                lobbySlots[i] = null;
                NetworkServer.Destroy(player.gameObject);
            }
        }

        OnLobbyServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        string loadedSceneName = SceneManager.GetSceneAt(0).name;
        if (loadedSceneName != m_LobbyScene)
        {
            return;
        }

        // check MaxPlayersPerConnection
        int numPlayersForConnection = 0;
        foreach (var player in conn.playerControllers)
        {
            if (player.IsValid)
                numPlayersForConnection += 1;
        }

        byte slot = FindSlot();
        if (slot == Byte.MaxValue)
        {
            if (LogFilter.logWarn) { Debug.LogWarning("NetworkLobbyManager no space for more players"); }

            var errorMsg = new EmptyMessage();
            conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
            return;
        }

        var newLobbyGameObject = OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
        if (newLobbyGameObject == null)
        {
            newLobbyGameObject = (GameObject)Instantiate(lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
        }

        var newLobbyPlayer = newLobbyGameObject.GetComponent<LobbyPlayer>();
        newLobbyPlayer.slot = slot;
        lobbySlots[slot] = newLobbyPlayer;

        NetworkServer.AddPlayerForConnection(conn, newLobbyGameObject, playerControllerId);
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        var playerControllerId = player.playerControllerId;
        byte slot = player.gameObject.GetComponent<LobbyPlayer>().slot;
        lobbySlots[slot] = null;
        base.OnServerRemovePlayer(conn, player);

        foreach (var p in lobbySlots)
        {
            if (p != null)
            {
                p.GetComponent<LobbyPlayer>().readyToBegin = false;

                s_LobbyReadyToBeginMessage.slotId = p.slot;
                s_LobbyReadyToBeginMessage.readyState = false;
                NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, s_LobbyReadyToBeginMessage);
            }
        }

        OnLobbyServerPlayerRemoved(conn, playerControllerId);
    }

    public override void ServerChangeScene(string sceneName)
    {
        if (sceneName == m_LobbyScene)
        {
            foreach (var lobbyPlayer in lobbySlots)
            {
                if (lobbyPlayer == null)
                    continue;

                // find the game-player object for this connection, and destroy it
                var uv = lobbyPlayer.GetComponent<NetworkIdentity>();

                // They had this in their code but it seems to cause problems
                //NetworkServer.Destroy(uv.connectionToClient.playerControllers[0].gameObject);

                if (NetworkServer.active)
                {
                    // re-add the lobby object
                    lobbyPlayer.GetComponent<LobbyPlayer>().readyToBegin = false;
                    NetworkServer.ReplacePlayerForConnection(uv.connectionToClient, lobbyPlayer.gameObject, uv.playerControllerId);
                }
            }
        }
        base.ServerChangeScene(sceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName != m_LobbyScene)
        {
            // call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
            foreach (var pending in m_PendingPlayers)
            {
                SceneLoadedForPlayer(pending.conn, pending.lobbyPlayer);
            }
            m_PendingPlayers.Clear();
        }
    }

    void OnServerReadyToBeginMessage(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager OnServerReadyToBeginMessage"); }
        netMsg.ReadMessage(s_ReadyToBeginMessage);

        PlayerController lobbyController = netMsg.conn.playerControllers.First();

        // set this player ready
        var lobbyPlayer = lobbyController.gameObject.GetComponent<LobbyPlayer>();
        lobbyPlayer.readyToBegin = s_ReadyToBeginMessage.readyState;

        // tell every player that this player is ready
        var outMsg = new LobbyReadyToBeginMessage();
        outMsg.slotId = lobbyPlayer.slot;
        outMsg.readyState = s_ReadyToBeginMessage.readyState;
        NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, outMsg);

        // maybe start the game
        CheckReadyToBegin();
    }

    void OnServerSceneLoadedMessage(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager OnSceneLoadedMessage"); }

        netMsg.ReadMessage(s_SceneLoadedMessage);

        PlayerController lobbyController = netMsg.conn.playerControllers.First();

        SceneLoadedForPlayer(netMsg.conn, lobbyController.gameObject);
    }

    void OnServerReturnToLobbyMessage(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkLobbyManager OnServerReturnToLobbyMessage"); }

        ServerReturnToLobby();
    }

    public override void OnStartServer()
    {
        if (string.IsNullOrEmpty(m_LobbyScene))
        {
            if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager LobbyScene is empty. Set the LobbyScene in the inspector for the NetworkLobbyMangaer"); }
            return;
        }

        if (string.IsNullOrEmpty(m_PlayScene))
        {
            if (LogFilter.logError) { Debug.LogError("NetworkLobbyManager PlayScene is empty. Set the PlayScene in the inspector for the NetworkLobbyMangaer"); }
            return;
        }

        if (lobbySlots.Length == 0)
        {
            lobbySlots = new LobbyPlayer[maxPlayers];
        }

        NetworkServer.RegisterHandler(MsgType.LobbyReadyToBegin, OnServerReadyToBeginMessage);
        NetworkServer.RegisterHandler(MsgType.LobbySceneLoaded, OnServerSceneLoadedMessage);
        NetworkServer.RegisterHandler(MsgType.LobbyReturnToLobby, OnServerReturnToLobbyMessage);
    }
}
