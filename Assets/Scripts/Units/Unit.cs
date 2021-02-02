using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System;

public class Unit : NetworkBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    //Uses C# events to alert game of specific actions. All subscribed methods will react to these events
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    //Returns unit targeter component
    public Targeter getTargeter()
    {
        return targeter;
    }

    //Returns unit movement component
    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    #region Server

    public override void OnStartServer()
    {
        //TODO
        ServerOnUnitSpawned?.Invoke(this);
        //Subscribe to event
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        //TODO
        ServerOnUnitDespawned?.Invoke(this);
        //Unsubscribe from event
        health.ServerOnDie -= ServerHandleDie;
    }

    //[Server] means this method can only be called by the server
    [Server]
    private void ServerHandleDie()
    {
        //removes game object from the world
        NetworkServer.Destroy(gameObject);
    }
    #endregion

    #region Client

    //[Client] means this method is only called by the client
    [Client]

    //Select unit
    public void Select()
    {
        //Can only select units the current player has authority over
        if (!hasAuthority) { return; }
        onSelected?.Invoke();
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority) { return; }
        onDeselected?.Invoke();
    }

    //TODO
    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    //TODO
    public override void OnStopClient()
    {
        if (!hasAuthority) { return; }
        AuthorityOnUnitDespawned?.Invoke(this);
    }

    #endregion
}
