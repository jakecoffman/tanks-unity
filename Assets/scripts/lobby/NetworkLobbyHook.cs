using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
		Tank tank = gamePlayer.GetComponent<Tank>();

        tank.playerName = lobby.playerName;
        tank.color = lobby.playerColor;
    }
}
