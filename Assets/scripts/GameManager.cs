using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameManager : NetworkBehaviour {
    public static GameManager instance = null;
    public BoardManager boardScript;

    private int level = 3;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

	public override void OnStartServer() {
        if (!isServer)
        {
            return;
        }
        boardScript = GetComponent<BoardManager>();
        boardScript.SetupScene(level);
    }
}
