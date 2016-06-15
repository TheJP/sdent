﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class Saloon : RtsTrainingBuilding
{
    public const int InventorySize = int.MaxValue;

    public GameObject workerPrefab;
    public GameObject merchantPrefab;
    public GameObject straightRailPrefab;

    private readonly Inventory inventory = new Inventory(InventorySize);
    private readonly List<IAbility> abilities = new List<IAbility>();
    public override IEnumerable<IAbility> Abilities
    {
        get { return abilities; }
    }

    public override Buildings Type
    {
        get { return Buildings.Saloon; }
    }

    public override Inventory Inventory
    {
        get { return inventory; }
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        if (hasAuthority)
        {
            Inventory.AddResources(ResourceTypes.Food, 200);
            CmdSpawnStraightRail();
        }
    }

    [Command]
    private void CmdSpawnStraightRail()
    {
        FindObjectOfType<EntityControl>().SpawnEntity(straightRailPrefab, new Vector3(transform.position.x + 12f, 0f, transform.position.z), Client);
    }

    public Saloon()
    {
        abilities.Add(new TrainUnit(this, saloon => (saloon as Saloon).workerPrefab, new[] { ResourceTypes.Food.Times(40) },
            "Worker", "Train a worker, which can build structures and harvest resources.", KeyCode.W, "TrainWorker"));
        abilities.Add(new TrainUnit(this, saloon => (saloon as Saloon).merchantPrefab, new[] { ResourceTypes.Plank.Times(100), ResourceTypes.Gold.Times(50) },
            "Merchant", "Train a merchant, which can transport resources between storage buildings.", KeyCode.E, "TrainMerchant"));
    }
}
