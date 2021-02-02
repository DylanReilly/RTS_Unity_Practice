using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    private List<UnitBase> bases = new List<UnitBase>();

    #region Server


    public override void OnStartServer()
    {
        //Subscribes to event ServerOnBaseSpawned
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        //Unsubscribes from event
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        //Adds new base to list
        bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        //removes base from list
        bases.Remove(unitBase);

        //if base count is not equal to 1, do nothing. Otherwise end the game
        if (bases.Count != 1) { return; }

        Debug.Log("Game Over");
    }
    #endregion

    #region Client

    #endregion
}
