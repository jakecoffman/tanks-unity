using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Level1 : MonoBehaviour {
    public GameObject floorPrefab;

    int columns = 16;
    int rows = 16;

    void Start()
    {
        GenerateGround();
    }

    void GenerateGround()
    {
        var parent = GameObject.Find("Level1").transform;
        for(var i=1; i<columns-1; i++)
        {
            for (var j=1; j<rows-1; j++)
            {
                var floor = Instantiate(floorPrefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;
                floor.transform.SetParent(parent);
            }
        }
    }
}
