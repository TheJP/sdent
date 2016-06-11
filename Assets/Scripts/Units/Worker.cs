using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class Worker : RtsUnit
{
    public const double WorkDistance = 12;

    private enum States { Idle, Traveling, Building }

    public GameObject storageHousePrefab;

    /// <summary>Building or Resource, which this worker is assigned to. This is not null only for the client with authority.</summary>
    private RtsEntity assignedWork = null;
    private States workerState = States.Idle;

    public Worker()
    {
        AddAbility(new BuildBuilding("Storage House", "Build a storage house, where workers can load off their resources.", KeyCode.Q, this, () => storageHousePrefab));
    }

    [ClientRpc]
    public void RpcAssignWork(GameObject entity)
    {
        if (hasAuthority) { assignedWork = entity.GetComponent<RtsEntity>(); }
    }

    private void DoAssignedWork(NavMeshAgent agent)
    {
        if ((assignedWork.transform.position - transform.position).sqrMagnitude <= WorkDistance * WorkDistance)
        {
            if (assignedWork is ConstructionSite) { workerState = States.Building; }
            else { workerState = States.Idle; }
        }
        else if(workerState != States.Traveling)
        {
            if (agent.SetDestination(assignedWork.transform.position))
            {
                agent.Resume();
                workerState = States.Traveling;
            }
        }
    }

    [Command]
    private void CmdBuild(GameObject assignedWork)
    {
        var constructionSite = assignedWork.GetComponent<ConstructionSite>();
        constructionSite.state += 1f;
    }

    protected override void Update()
    {
        base.Update();
        var agent = GetComponent<NavMeshAgent>();
        if (hasAuthority)
        {
            switch (workerState)
            {
                case States.Idle:
                    if (assignedWork != null) { DoAssignedWork(agent); }
                    break;
                case States.Traveling:
                    if (!agent.pathPending && (agent.destination - transform.position).sqrMagnitude <= WorkDistance * WorkDistance)
                    {
                        //Completed path: transition to work / idle
                        if (assignedWork == null) { workerState = States.Idle; }
                        else { DoAssignedWork(agent); }
                    }
                    break;
                case States.Building:
                    if (assignedWork == null) { workerState = States.Idle; }
                    else { CmdBuild(assignedWork.gameObject); }
                    break;
            }
        }
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
