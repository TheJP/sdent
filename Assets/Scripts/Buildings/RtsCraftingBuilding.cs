﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Base class for every building, which can craft certain recipes.</summary>
public abstract class RtsCraftingBuilding : RtsBuilding, IHasInventory
{
    /// <summary>Factor, how much more space a crafting building has, than it needs for one recipe (including output).</summary>
    public const int CraftingSpaceFactor = 5;

    public Transform carrierSpawn;

    private float lastCraftingTime = 0f;
    private Carrier assignedCarrier;

    public abstract IEnumerable<Recipe> Recipes { get; }

    public Inventory Inventory { get; private set; }

    public RtsCraftingBuilding()
    {
        var exactSpace = Recipes.SelectMany(r => r.Input.Union(r.Output)).Sum(tuple => tuple.Amount);
        this.Inventory = new FilteredInventory(CraftingSpaceFactor * exactSpace, Recipes);
    }

    public void SetCarrier(Carrier carrier)
    {
        this.assignedCarrier = carrier;
    }

    [Command]
    private void CmdSpawnCarrier()
    {
        var carrier = (GameObject)Instantiate(FindObjectOfType<EntityControl>().carrierPrefab, carrierSpawn.position, Quaternion.identity);
        carrier.GetComponent<Carrier>().AssignedWork = gameObject;
        NetworkServer.SpawnWithClientAuthority(carrier, Client);
    }


    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        if (hasAuthority) { CmdSpawnCarrier(); }
    }

    protected override void Update()
    {
        base.Update();
        foreach (var recipe in Recipes)
        {
            //Has time and resources to do this recipe?
            if (lastCraftingTime + recipe.CraftingTime < Time.time &&
                !recipe.Input.Any(input => Inventory[input.Resource] < input.Amount) &&
                Inventory.CanAddResources(recipe.Output))
            {
                //Craft this recipe
                var removed = true;
                foreach (var input in recipe.Input) { removed = removed && Inventory.RemoveResources(input.Resource, input.Amount); }
                if (!removed) { continue; }
                foreach (var output in recipe.Output) { Inventory.AddResources(output.Resource, output.Amount); }
                lastCraftingTime = Time.time;
            }
        }
    }
}
