using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ConstructionSite : RtsBuilding
{
    private GameObject finalBuildingPrefab;
    private readonly HashSet<Worker> buildingWorkers = new HashSet<Worker>();
    private bool finishedBuilding = false;

    [ClientRpc]
    public void RpcSetFinalBuilding(GameObject finalBuildingPrefab)
    {
        Debug.Log(finalBuildingPrefab);
        //Everyone is allowed to know, which building this construction site will be when done
        this.finalBuildingPrefab = finalBuildingPrefab;
    }

    [Command]
    public void CmdFinishBuilding(GameObject buildingPrefab)
    {
        FindObjectOfType<EntityControl>().SpawnEntity(buildingPrefab, transform.position, Client);
        CmdDie();
    }

    public void WorkerStartBuilding(Worker worker) { buildingWorkers.Add(worker); }
    public void WorkerStopBuilding(Worker worker) { buildingWorkers.Remove(worker); }

    protected override void Start()
    {
        base.Start();
        finishedBuilding = false;
        buildingWorkers.Clear();
        state = 1f;
    }

    protected override void Update()
    {
        base.Update();
        if (hasAuthority && !finishedBuilding)
        {
            state += buildingWorkers.Count * Worker.WorkerBuildingSpeed;
            if (state >= MaxState)
            {
                finishedBuilding = true;
                foreach (var worker in buildingWorkers) { worker.FinishedBuilding(); }
                buildingWorkers.Clear();
                CmdFinishBuilding(finalBuildingPrefab);
            }
        }
    }
}
