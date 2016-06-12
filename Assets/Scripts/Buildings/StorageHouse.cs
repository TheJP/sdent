using UnityEngine;
using System.Collections;
using System;

public class StorageHouse : RtsBuilding, IHasInventory
{
    public const int InventorySize = 1000;

    private readonly Inventory inventory = new Inventory(InventorySize);
    public Inventory Inventory
    {
        get { return inventory; }
    }

    public override Buildings Type
    {
        get { return Buildings.StorageHouse; }
    }
}
