using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action<string> ClientOnGameOver;

    //Creates list of bases
    private List<UnitBase> bases = new List<UnitBase>();

    #region Server

    public override void OnStartServer()
    {
        //Subscribes to events
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        //Unsubscribes from events
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        //If there are more than 1 bases, do nothing. Otherwise end the game
        if (bases.Count != 1) { return; }

        //When there is only 1 base left get its owners ID
        int playerID = bases[0].connectionToClient.connectionId;

        //$ allows insertion of variables inside {} without using +
        RpcGameOver($"Player {playerID}");
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        //Triggers GameOver event
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
