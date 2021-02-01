using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    //Used to control fire rate
    private float lastFireTime;

    //TODO
    [ServerCallback]

    private void Update()
    {
        //Gets target position
        Targetable target = targeter.getTarget();

        //if no target found do nothing
        if (target == null) { return; }

        //If cant fire at target do nothing
        if (!CanFireAtTarget()) { return; }

        //Gets target rotation
        Quaternion targetRotation = 
            Quaternion.LookRotation(target.transform.position - transform.position);

        //Rotates unit to face target
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //Controls fire rate relative to last time fired
        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            //sets projectile rotation to point at target
            Quaternion projectileRotation = Quaternion.LookRotation(
                target.GetAimAtPoint().position - projectileSpawnPoint.position);

            //create projectile
            GameObject projectiltInstance = Instantiate(
                projectilePrefab, projectileSpawnPoint.position, projectileRotation);

            //Spawn projectile on network, give authority to current player
            NetworkServer.Spawn(projectiltInstance, connectionToClient);

            //update last time fired to current time
            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget()
    {
        //Checks if target is in range
        return (targeter.getTarget().transform.position - transform.position).sqrMagnitude
            <= fireRange * fireRange;
    }
}
