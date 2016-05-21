using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class RtsUnit : RtsEntity
{
    public override void DoRightClickAction(Vector3 position)
    {
        //Only the local (authorized) player is allowed to move units.
        if (hasAuthority) { GetComponent<NavMeshAgent>().SetDestination(position); }
    }
}
