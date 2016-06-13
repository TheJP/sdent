using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Linq;
using UnityStandardAssets.Characters.ThirdPerson;

public class Carrier : NetworkBehaviour, IHasInventory
{
    public const int InventorySize = 20;

    private readonly Inventory inventory = new Inventory(InventorySize);

    [SyncVar(hook = "SetAssignedWork")]
    private GameObject assignedWork;
    private RtsCraftingBuilding assignedBuilding;

    private void SetAssignedWork(GameObject assignedWork)
    {
        this.AssignedWork = assignedWork;
    }

    [SyncVar(hook = "SetTarget")]
    private Transform target;

    private void SetTarget(Transform target)
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

    public Transform Target 
    {
        get { return target; }
        set
        {
            target = value;
            GetComponent<AICharacterControl>().SetTarget(value);
        }
    }

    public Inventory Inventory
    {
        get { return inventory; }
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Target = FindObjectsOfType<Saloon>().FirstOrDefault(saloon => saloon.hasAuthority).transform;
    }
}
