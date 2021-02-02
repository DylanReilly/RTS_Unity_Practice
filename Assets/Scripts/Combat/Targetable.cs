using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : NetworkBehaviour
{
    [SerializeField] private Transform aimAtPoint = null;

    //Returns the position enemy units will aim for when attacking this target
    public Transform GetAimAtPoint()
    {
        return aimAtPoint;
    }
}
