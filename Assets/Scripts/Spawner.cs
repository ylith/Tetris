using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void spawnPiece()
    {
        var gameobjects = GameObject.FindGameObjectsWithTag("Piece");
        foreach (var gameobject in gameobjects)
        {
            Debug.Log(gameobject);
        }
    }
}
