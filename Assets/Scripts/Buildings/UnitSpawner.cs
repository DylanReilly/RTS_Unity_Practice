using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    #region Server

    public override void OnStartServer()
    {
        //Subscribe to event
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        //Unsubscribe from event
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        //NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        //Spawn unit using unit spawn point component
        GameObject unitInstance = Instantiate(unitPrefab, 
            unitSpawnPoint.position, 
            unitSpawnPoint.rotation);

        //Spawn unit on the network
        NetworkServer.Spawn(unitInstance, connectionToClient);
    }
    #endregion

    #region Client
    //Spawn unit when clicking on base
    public void OnPointerClick(PointerEventData eventData)
    {
        //If the left button wasnt pressed do nothing
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        //If the player doesnt have authority over the target do nothing
        if(!hasAuthority) { return; }

        //Spawn unit
        CmdSpawnUnit();
    }


    #endregion
}
