using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandHandler : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    //Camera object must be created to use raycasting
    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    //Update is called automatically every frame by unity
    private void Update()
    {
        //Do nothing if RMB wasnt pressed
        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        //Create ray for raycast. Takes mouse position for raycast
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        //Do nothing if the raycast hits nothing
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

        //If raycast hits something, check if it has a targetable component
        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            //If player has authority over the unit
            if (target.hasAuthority)
            {
                //Try move unit to point
                TryMove(hit.point);
                return;
            }
            //Try target what the raycast hit
            TryTarget(target);
            return;
        }

        TryMove(hit.point);
    }

    private void TryMove(Vector3 point)
    {
        //for every selected unit
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            //Move unit to point
            //CmdMove is the client requesting the server to move them
            unit.GetUnitMovement().CmdMove(point);
        }
    }

    //Similar to TryMove but for targeting enemy units
    private void TryTarget(Targetable target)
    {
        foreach (Unit unit in unitSelectionHandler.SelectedUnits)
        {
            unit.getTargeter().CmdSetTarget(target.gameObject);
        }
    }

    private void ClientHandleGameOver(string winnerName)
    {
        //Disables this script, which in turn disables the update loop and stops player interacting
        enabled = false;
    }
}
