using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class Worker : RtsUnit
{
    public GameObject storageHousePrefab;

    /// <summary>Building or Resource, which this worker is assigned to. This is not null only for the client with authority.</summary>
    private RtsEntity assignedWork = null;

    public Worker()
    {
        AddAbility(new BuildBuilding("Storage House", "Build a storage house, where workers can load off their resources.", KeyCode.Q, this, () => storageHousePrefab));
    }

    [ClientRpc]
    public void RpcAssignWork(GameObject entity)
    {
        if (hasAuthority) { assignedWork = entity.GetComponent<RtsEntity>(); }
    }

    protected override void Update()
    {
        base.Update();
        //TODO: Worker state machine
    }

    [Command]
    private void CmdBuildBuilding(GameObject buildingPrefab)
    {
        var ground = Utility.RayMouseToGround();
        if (ground.HasValue)
        {
            //TODO: entity avoidance for new buildings
            FindObjectOfType<EntityControl>().BuildConstructionSite(buildingPrefab, ground.Value, Client, gameObject);
        }
    }

    private class BuildBuilding : AbilityBase
    {
        private Worker worker;
        private Func<GameObject> finalBuildingPrefab;

        public BuildBuilding(string name, string lore, KeyCode key, Worker worker, Func<GameObject> finalBuildingPrefab) : base(name, lore, key)
        {
            this.worker = worker;
            this.finalBuildingPrefab = finalBuildingPrefab;
        }

        public override void Execute()
        {
            if (worker.hasAuthority) { worker.CmdBuildBuilding(finalBuildingPrefab()); }
        }
    }
}
