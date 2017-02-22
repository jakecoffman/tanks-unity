using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Level1 : MonoBehaviour {
    int columns = 16;
    int rows = 16;

    void Start()
    {
        GenerateGround();
    }

    void GenerateGround()
    {
        for(var i=1; i<columns-1; i++)
        {
            for (var j=1; j<rows-1; j++)
            {
            }
        }
    }
}
