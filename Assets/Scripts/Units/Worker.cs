using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class Worker : RtsUnit
{

    /// <summary>Building or Resource, which this worker is assigned to. This is not null only for the client with authority.</summary>
    private RtsEntity assignedWork = null;

    [ClientRpc]
    public void RpcAssignWork(GameObject entity)
    {
        if (hasAuthority) { assignedWork = entity.GetComponent<RtsEntity>(); }
    }

    private class BuildStorageHouse : AbilityBase
    {
        public BuildStorageHouse() : base("Storage House", "Build a storage house, where workers can load off their resources.", KeyCode.Q) { }

        public override void Execute()
        {
            //TODO: Build building
        }
    }
}
