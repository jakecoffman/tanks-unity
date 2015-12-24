using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameManager : NetworkBehaviour {

    public BoardManager boardScript;

    private int level = 3;

	public override void OnStartServer() {
        boardScript = GetComponent<BoardManager>();
        boardScript.SetupScene(level);
    }
}
