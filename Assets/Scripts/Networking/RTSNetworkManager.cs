using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Extends network manager for network functionality
public class RTSNetworkManager : NetworkManager
{
    //[SerializedField] allows for drag-and-drop references in Unity GUI
    [SerializeField] private GameObject unitSpawnerPrefab = null;
    //GameOverHandler is used for alerting the network manager to the end of the game
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        ClientOnDisconnected?.Invoke();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //Performs all base logic of the overrided class
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();
        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
            ));
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
