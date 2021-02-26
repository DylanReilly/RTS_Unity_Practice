using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings = new Building[0];
    [SerializeField] private float buildingRangeLimit = 5f;

    //Hook is used in syncvars, Whenever the variable is changed the hooked method is called
    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))] private int resources = 500;
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner = false;

    public event Action<int> ClientOnResourcesUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    private Color teamColor = new Color();
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }

    //Returns a list of the players units
    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    public Color GetTeamColor()
    {
        return (teamColor);
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }

    public int getResources()
    {
        return resources;
    }

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        //Check if building is colliding with existing building
        if (Physics.CheckBox(
            point + buildingCollider.center,
            buildingCollider.size / 2,
            Quaternion.identity,
            buildingBlockLayer))
        {
            return false;
        }

        //Check if new building is in range of existing building
        foreach (Building building in myBuildings)
        {
            if ((point - building.transform.position).sqrMagnitude
                <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }

        return false;
    }

    #region Server

    //subscribes to events, meaning it will listen and react to these events
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerhandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerhandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
    }

    //Unsubscribes from events, stops listening
    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerhandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerhandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        teamColor = newTeamColor;
    }

    [Server]
    public void SetResources(int newResources)
    {
        resources = newResources;
    }

    [Command]
    public void CmdStartGame()
    {
        if (!isPartyOwner) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }


    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;
        //Loop over the list of buildings for matching ID
        foreach (Building building in buildings)
        {
            if (building.GetId() == buildingId)
            {
                buildingToPlace = building;
                //When building is found exit the loop
                break;
            }
        }
        if (buildingToPlace == null) { return; }

        //Check to make sure player has enough money to make building
        if (resources < buildingToPlace.GetPrice()) { return; }

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingCollider, point)) { return; }

        //Create the instance of the building
        GameObject buildingInstance = 
            Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);

        //Spawn the building on the network and give ownership to the player
        NetworkServer.Spawn(buildingInstance, connectionToClient);

        //Take building price away from total resources
        SetResources(resources - buildingToPlace.GetPrice());
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

    //Adds building to specific players list
    private void ServerHandleBuildingSpawned(Building building)
    {
        //Checks the units client ID and only gives authority to the matching client
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Add(building);
    }

    //Removes building from specific players list
    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Remove(building);
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
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStartClient()
    {
        if (NetworkServer.active) { return; }
        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
    }

    //Unsubscribe from events
    public override void OnStopClient()
    {
        //Ignore if you are the server
        if (!isClientOnly) { return; }

        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        if (!hasAuthority) { return; }

        Unit.AuthorityOnUnitSpawned -= AuthorityhandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityhandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        if (!hasAuthority) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
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

    //Add Building to player list
    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    //Remove Building from player list
    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    private void ClientHandleResourcesUpdated(int oldResource, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    #endregion
}
