using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    private Targetable target;

    public override void OnStartServer()
    {
        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    private void ServerHandlePlayerDie(int connectionID)
    { 
        if(connectionToClient.connectionId != connectionID) { return; }

        ClearTarget();
    }
    //Returns the current target
    public Targetable getTarget()
    {
        return target;
    }


    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        //If the new target has no targetable component, do nothing
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }

        target = newTarget;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }
}
