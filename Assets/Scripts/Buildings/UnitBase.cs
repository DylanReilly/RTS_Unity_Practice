using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private Health health = null;

    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawned;

    #region Server
    public override void OnStartServer()
    {
        health.ServerOnDie += HandleBaseDeath;

        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= HandleBaseDeath;

        ServerOnBaseDespawned?.Invoke(this);
    }

    private void HandleBaseDeath()
    {
        NetworkServer.Destroy(gameObject);
    }
    #endregion

    #region Client

    #endregion
}
