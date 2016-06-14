﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Linq;

public class ConstructionSite : RtsBuilding, IHasInventory
{
    public GameObject[] buildingPrefabs;

    private readonly Dictionary<Buildings, GameObject> prefabDictionary = new Dictionary<Buildings, GameObject>();

    [SyncVar]
    private Buildings finalBuilding;

    private readonly HashSet<Worker> buildingWorkers = new HashSet<Worker>();
    private Inventory inventory = new Inventory(0);
    private bool finishedBuilding = false;

    public Buildings FinalBuilding
    {
        get { return finalBuilding; }
        set { finalBuilding = value; }
    }

    public Inventory Inventory
    {
        get { return inventory; }
    }

    public override Buildings Type
    {
        get { return Buildings.ConstructionSite; }
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

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        var costs = prefabDictionary[FinalBuilding].GetComponent<RtsBuilding>().BuildingCosts;
        inventory = new FilteredInventory(costs.ToDictionary(tuple => tuple.Resource, tuple => tuple.Amount));
    }

    [Command]
    private void CmdBuild(float addToState)
    {
        state += addToState;
        if (state >= MaxState)
        {
            RpcFinishedBuilding();
            FindObjectOfType<EntityControl>().SpawnEntity(prefabDictionary[FinalBuilding], transform.position, Client);
            CmdDie();
        }
    }

    [ClientRpc]
    private void RpcFinishedBuilding()
    {
        finishedBuilding = true;
        foreach (var worker in buildingWorkers) { worker.FinishedBuilding(); }
        buildingWorkers.Clear();
    }

    protected override void Update()
    {
        base.Update();
        if (hasAuthority && !finishedBuilding)
        {
            CmdBuild(buildingWorkers.Count * Worker.WorkerBuildingSpeed * Time.deltaTime);
        }
    }
}
