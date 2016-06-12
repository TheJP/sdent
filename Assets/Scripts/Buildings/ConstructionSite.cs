﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class ConstructionSite : RtsBuilding
{
    public GameObject[] buildingPrefabs;

    private readonly Dictionary<Buildings, GameObject> prefabDictionary = new Dictionary<Buildings, GameObject>();

    [SyncVar]
    private Buildings finalBuilding;

    private readonly HashSet<Worker> buildingWorkers = new HashSet<Worker>();
    private bool finishedBuilding = false;

    public Buildings FinalBuilding
    {
        get { return finalBuilding; }
        set { finalBuilding = value; }
    }

    public override Buildings Type
    {
        get { return Buildings.ConstructionSite; }
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
        foreach(var prefab in buildingPrefabs)
        {
            prefabDictionary.Add(prefab.GetComponent<RtsBuilding>().Type, prefab);
        }
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
                CmdFinishBuilding(prefabDictionary[FinalBuilding]);
            }
        }
    }
}
