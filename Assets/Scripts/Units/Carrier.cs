using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Linq;
using UnityStandardAssets.Characters.ThirdPerson;

public class Carrier : NetworkBehaviour, IHasInventory
{
    public const int InventorySize = 20;
    public const float LoadingDistance = 12f;

    private enum States { Idle, WalkingToStorage, WalkingToWork }

    private readonly Inventory inventory = new Inventory(InventorySize);
    private States carrierState = States.Idle;

    [SyncVar(hook = "SetAssignedWork")]
    private GameObject assignedWork;
    private RtsCraftingBuilding assignedBuilding;

    private void SetAssignedWork(GameObject assignedWork)
    {
        this.AssignedWork = assignedWork;
    }

    [SyncVar(hook = "SetTarget")]
    private GameObject target;

    private void SetTarget(GameObject target)
    {
        this.Target = target;
    }

    [Command]
    private void CmdSetTarget(GameObject target)
    {
        this.Target = target;
    }

    public GameObject AssignedWork {
        get { return assignedWork; }
        set
        {
            assignedWork = value;
            assignedBuilding = value.GetComponent<RtsCraftingBuilding>();
            assignedBuilding.SetCarrier(this);
        }
    }

    public GameObject Target 
    {
        get { return target; }
        set
        {
            target = value;
            GetComponent<AICharacterControl>().SetTarget(value.transform);
        }
    }

    public Inventory Inventory
    {
        get { return inventory; }
    }

    private GameObject FindNearestStorage()
    {
        return Utility.FindNearestStorage(FindObjectOfType<EntityControl>().Entities, transform.position).gameObject;
    }

    /// <summary>Load as much output goods from the crafting building to the carrier as possible.</summary>
    private void LoadCarrierFromWork()
    {
        foreach (var output in assignedBuilding.Recipes.SelectMany(recipe => recipe.Output))
        {
            var amount = Math.Min(assignedBuilding.Inventory[output.Resource], Inventory.SpaceAvailable - Inventory.Count());
            if (amount > 0 && assignedBuilding.Inventory.RemoveResources(output.Resource, amount))
            {
                Inventory.AddResources(output.Resource, amount);
            }
            if (Inventory.Count() >= Inventory.SpaceAvailable) { break; }
        }
    }

    /// <summary>Offload as much resources as possible.</summary>
    /// <param name="targetInventory"></param>
    private void OffloadCarrierToInventory(Inventory targetInventory)
    {
        foreach (var resource in Inventory.Where(tuple => tuple.Value > 0).ToList())
        {
            var amount = Math.Min(resource.Value, targetInventory.FreeSpace);
            if (amount > 0 && targetInventory.AddResources(resource.Key, amount))
            {
                Inventory.RemoveResources(resource.Key, amount);
            }
        }
    }

    /// <summary>Load needed input materials from storage to carrier.</summary>
    /// <param name="storageInventory"></param>
    private void LoadCarrierFromStorage(Inventory storageInventory)
    {
        foreach (var input in assignedBuilding.Recipes.SelectMany(recipe => recipe.Input))
        {
            var amount = Math.Min(storageInventory[input.Resource], Math.Min((RtsCraftingBuilding.CraftingSpaceFactor * input.Amount) - assignedBuilding.Inventory[input.Resource], Inventory.FreeSpace));
            if (amount > 0 && storageInventory.RemoveResources(input.Resource, amount))
            {
                Inventory.AddResources(input.Resource, amount);
            }
            if (Inventory.Count() >= Inventory.SpaceAvailable) { break; }
        }
    }

    void Update()
    {
        if (!hasAuthority || assignedBuilding == null) { return; }
        switch (carrierState)
        {
            case States.Idle:
                LoadCarrierFromWork();
                Target = FindNearestStorage(); //Lag compensation: set target on client with authority here already
                CmdSetTarget(Target);
                carrierState = States.WalkingToStorage;
                break;
            case States.WalkingToStorage:
                if ((Target.transform.position - transform.position).sqrMagnitude <= LoadingDistance * LoadingDistance)
                {
                    var storageInventory = Target.gameObject.GetComponent<IHasInventory>().Inventory;
                    OffloadCarrierToInventory(storageInventory);
                    LoadCarrierFromStorage(storageInventory);
                    Target = AssignedWork; //Lag compensation: set target on client with authority here already
                    CmdSetTarget(Target);
                    carrierState = States.WalkingToWork;
                }
                break;
            case States.WalkingToWork:
                if ((assignedBuilding.transform.position - transform.position).sqrMagnitude <= LoadingDistance * LoadingDistance)
                {
                    OffloadCarrierToInventory(assignedBuilding.Inventory);
                    LoadCarrierFromWork();
                    Target = FindNearestStorage(); //Lag compensation: set target on client with authority here already
                    CmdSetTarget(Target);
                    carrierState = States.WalkingToStorage;
                }
                break;
        }
    }
}
