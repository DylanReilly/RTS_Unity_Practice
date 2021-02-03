using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health = null;

    public static event Action<int> ServerOnPlayerDie;
    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawned;

    #region Server
    public override void OnStartServer()
    {
        //Subscribe to event
        health.ServerOnDie += HandleBaseDeath;
        
        //Invoke triggers the event
        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        //Unsubscribe from event
        health.ServerOnDie -= HandleBaseDeath;

        ServerOnBaseDespawned?.Invoke(this);
    }

    private void HandleBaseDeath()
    {
        //Event when the players base is destroyed to stop them playing
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(gameObject);
    }
    #endregion

    #region Client

    #endregion
}
