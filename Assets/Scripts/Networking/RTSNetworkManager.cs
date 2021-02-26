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
    [SerializeField] private GameObject unitBasePrefab = null;
    //GameOverHandler is used for alerting the network manager to the end of the game
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    private bool isGameInProgress = false;

    #region Server
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (!isGameInProgress) { return; }

        conn.Disconnect();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();

        isGameInProgress = false;
    }

    public void StartGame()
    {
        if (Players.Count < 2) { return; }

        isGameInProgress = true;

        ServerChangeScene("Scene_Map_01");
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //Performs all base logic of the overrided class
        base.OnServerAddPlayer(conn);

        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        player = conn.identity.GetComponent<RTSPlayer>();
        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
            ));

        player.SetPartyOwner(Players.Count == 1);
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

            foreach(RTSPlayer player in Players)
            {
                GameObject baseInstance = Instantiate(
                    unitBasePrefab, 
                    GetStartPosition().position, 
                    Quaternion.identity);

                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }
    #endregion

    #region Client
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

    public override void OnStopClient()
    {
        base.OnStopClient();
    }
    #endregion
}
