using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private List<Unit> myUnits = new List<Unit>();

    //Returns a list of the players units
    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    #region Server

    //subscribes to events, meaning it will listen and react to these events
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerhandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerhandleUnitDespawned;
    }

    //Unsubscribes from events, stops listening
    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerhandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerhandleUnitDespawned;
    }

    //Adds unit to specific players list
    private void ServerhandleUnitSpawned(Unit unit)
    {
        //Checks the units client ID and only gives authority to the matching client
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        
        myUnits.Add(unit);
    }

    //Removes unit from specific players list
    private void ServerhandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);
    }

    #endregion

    #region Client

    //Subscribes to events
    public override void OnStartAuthority()
    {
        //Ignore if you are the server
        if (NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityhandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityhandleUnitDespawned;
    }

    //Unsubscribe from events
    public override void OnStopClient()
    {
        //Ignore if you are the server
        if (!isClientOnly || !hasAuthority) { return; }

        Unit.AuthorityOnUnitSpawned -= AuthorityhandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityhandleUnitDespawned;
    }

    //Add unit to player list
    private void AuthorityhandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    //Remove unit from player list
    private void AuthorityhandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    #endregion
}
