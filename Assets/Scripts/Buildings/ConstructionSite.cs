using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Linq;

public class ConstructionSite : RtsBuilding, IHasInventory
{
    public GameObject[] buildingPrefabs;

    private readonly Dictionary<Buildings, GameObject> prefabDictionary = new Dictionary<Buildings, GameObject>();

    private Dictionary<Buildings, GameObject> PrefabDictionary
    {
        get
        {
            if (!prefabDictionary.Any())
            {
                foreach (var prefab in buildingPrefabs)
                {
                    prefabDictionary.Add(prefab.GetComponent<RtsBuilding>().Type, prefab);
                }
            }
            return prefabDictionary;
        }
    }

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

    public Dictionary<ResourceTypes, int> NeededResources
    {
        get
        {
            var costs = PrefabDictionary[FinalBuilding].GetComponent<RtsBuilding>().BuildingCosts;
            return costs.ToDictionary(tuple => tuple.Resource, tuple => tuple.Amount);
        }
    }

    public void WorkerStartBuilding(Worker worker) { buildingWorkers.Add(worker); }
    public void WorkerStopBuilding(Worker worker) { buildingWorkers.Remove(worker); }

    protected override void Start()
    {
        finishedBuilding = false;
        buildingWorkers.Clear();
        state = 1f;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        var entityControl = FindObjectOfType<EntityControl>();
        if (!entityControl.Entities.Contains(this)) { entityControl.AddEntity(this); }
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        inventory = new FilteredInventory(NeededResources);
    }

    [Command]
    private void CmdBuild(float addToState)
    {
        state += addToState;
        if (state >= MaxState)
        {
            FindObjectOfType<EntityControl>().SpawnEntity(PrefabDictionary[FinalBuilding], transform.position, Client);
            CmdDie();
        }
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
