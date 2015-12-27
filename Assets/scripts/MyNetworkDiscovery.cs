using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MyNetworkDiscovery : NetworkDiscovery {
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        NetworkManager.singleton.networkAddress = fromAddress;
        NetworkManager.singleton.StartClient();
    }
}
