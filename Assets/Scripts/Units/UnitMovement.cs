using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnPlayerDie += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandleGameOver;
    }

    private void ServerHandleGameOver(int connectionID)
    {
        if (connectionToClient.connectionId != connectionID) { return; }

        agent.ResetPath();
    }
    [ServerCallback]
    private void Update()
    {
        //Gets target coordinates from targetable component
        Targetable target = targeter.getTarget();
        if (target != null)
        {
            //checks if the unit is too far away from its target to attack, if so move unit to target position
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            //If the unit has reached its waypoint
            else if (agent.hasPath)
            {
                //reset path
                agent.ResetPath();
            }

            return;
        }

        //if unit has no path, return
        if (!agent.hasPath) { return; }

        //Stop unit move if its "close enough" to the waypoint
        if (agent.remainingDistance > agent.stoppingDistance) { return; }

        agent.ResetPath();
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

    //TODO
    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }
    #endregion
}
