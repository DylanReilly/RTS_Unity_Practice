using Mirror;
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

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //Performs all base logic of the overrided class
        base.OnServerAddPlayer(conn);

        //TODO
        GameObject unitSpawnerInstance = Instantiate(unitSpawnerPrefab, 
            conn.identity.transform.position, 
            conn.identity.transform.rotation);

        //Spawns unit spawner when player joins, conn gives authority to the joining player over the spawner
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
