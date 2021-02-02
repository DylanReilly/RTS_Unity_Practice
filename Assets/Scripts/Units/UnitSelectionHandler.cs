using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null;

    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Vector2 startPosition;

    private RTSPlayer player;
    private Camera mainCamera;

    //List of currently selected units used for multi/drag select
    public List<Unit> SelectedUnits { get; } = new List<Unit>();

    private void Start()
    {
        //set camera to be main camera for raycasts
        mainCamera = Camera.main;

        //Subscribe to event
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawn;
    }

    private void OnDestroy()
    {
        //Unsubscribe from event
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawn;
    }

    private void Update()
    {
        //TODO
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        //Start drag select when button is pressed
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        //End drag select when button is released
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }

        //If button is currently held, update selection area size
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
        }
    }

    private void StartSelectionArea()
    {
        //If ctrl is not pressed, deselect all units on click. This allows multiple selections when holding ctrl
        if (!Keyboard.current.ctrlKey.isPressed)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }
            SelectedUnits.Clear();
        }
        //Make selection area ui element visible
        unitSelectionArea.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        //Get mouse position on screen
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        //Calculates area size using the difference between the current mouse position and the start position
        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        //Gets absolute value so minus numbers wont matter
        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));

        //Returns the middle of the selection area
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
        
    }

    private void ClearSelectionArea()
    {
        //Disable select area UI element
        unitSelectionArea.gameObject.SetActive(false);

        //TODO
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            //Create raycast
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            //Return if ray hits nothing
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }

            //If the hit isnt a unit do nothing
            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) { return; }

            //If player doesn't have authority over unit do nothing
            if (!unit.hasAuthority) { return; }

            //Add selected unit to list
            SelectedUnits.Add(unit);

            //Select all units in the list
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }

            return;
        }

        //TODO
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (Unit unit in player.GetMyUnits())
        {
            //TODO
            if (SelectedUnits.Contains(unit)) { continue; }

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            //Checks if unit is inside selection area
            if (screenPosition.x > min.x &&
                screenPosition.x < max.x &&
                screenPosition.y > min.y &&
                screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private void AuthorityHandleUnitDespawn(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }
}
