using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private int damageToDeal = 20;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;

    void Start()
    {
        //sets the velocity of the projectile using the launch force
        rb.velocity = transform.forward * launchForce;
    }

    //TODO
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        //Check if target has the same client connection as projectile (friendly fire) if true, do nothing
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            if (networkIdentity.connectionToClient == connectionToClient) { return; }
        }

        //If target has health component, reduce health and destroy projectile
        if (other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damageToDeal);

            DestroySelf();
        }
    }

    //destroys projectile
    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
