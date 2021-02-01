using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private List<Unit> myUnits = new List<Unit>();

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerhandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerhandleUnitDespawned;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerhandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerhandleUnitDespawned;
    }

    private void ServerhandleUnitSpawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
        
        myUnits.Add(unit);
    }

    private void ServerhandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityhandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityhandleUnitDespawned;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) { return; }

        Unit.AuthorityOnUnitSpawned -= AuthorityhandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityhandleUnitDespawned;
    }

    private void AuthorityhandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityhandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    #endregion
}
