using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class StorageHouse : RtsBuilding, IHasInventory
{
    public const int InventorySize = int.MaxValue;

    private readonly Inventory inventory = new Inventory(InventorySize);
    public Inventory Inventory
    {
        get { return inventory; }
    }

    public override Buildings Type
    {
        get { return Buildings.StorageHouse; }
    }

    public override IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return new[] { ResourceTypes.Planks.Times(200), ResourceTypes.Steel.Times(100), ResourceTypes.Bricks.Times(100) }; }
    }
}
