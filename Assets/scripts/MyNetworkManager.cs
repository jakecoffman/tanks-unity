using UnityEngine;using UnityEngine.Networking;using System.Collections;public class MyNetworkManager : NetworkManager {    public override void OnStartServer()    {            }    void Update()    {            }    void OnGUI()    {        var players = GameObject.FindGameObjectsWithTag("Player");        GUI.Label(new Rect(10, 100, 300, 20), "PLayer count: " + players.Length);    }    public override void OnClientConnect(NetworkConnection conn)    {        Debug.Log("ClientConnected");    }    public override void OnClientDisconnect(NetworkConnection conn)    {        Debug.Log("Client disconnected");    }}