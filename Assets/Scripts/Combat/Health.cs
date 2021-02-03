using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    //TODO
    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    public event Action ServerOnDie;

    public event Action<int, int> ClientOnHealthUpdate;

    #region Server
    //When a unit is create set its health to max
    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionID)
    {
        if (connectionToClient.connectionId != connectionID) { return; }
        DealDamage(currentHealth);
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        //If unit has no health do nothing
        if (currentHealth == 0) { return; }

        //Reduce health by damage amount
        currentHealth -= damageAmount;

        //if the health drops below 0, set it to 0
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        //If unit is not dead, stop here
        if (currentHealth != 0) { return; }

        //TODO
        ServerOnDie?.Invoke();

    }
    #endregion

    #region Client

    //Update health on clients
    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdate?.Invoke(newHealth, maxHealth);
    }

    #endregion
}
