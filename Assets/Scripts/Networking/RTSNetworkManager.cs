﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitSpawnerPrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        GameObject unitSpawnerInstance = Instantiate(unitSpawnerPrefab, 
            conn.identity.transform.position, 
            conn.identity.transform.rotation);

        NetworkServer.Spawn(unitSpawnerInstance, conn);
    }

    //Called when the scene is changed
    public override void OnServerSceneChanged(string sceneName)
    {
        //Checks the name to see if its a map, menu etc
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            //Spawns the game over handler
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            //Spawns handler on network for everyone
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
        }
    }
}