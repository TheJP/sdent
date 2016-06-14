using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class Saloon : RtsTrainingBuilding, IHasInventory
{
    public const int InventorySize = int.MaxValue;

    public GameObject workerPrefab;
    public GameObject merchantPrefab;
    public Transform spawnPoint;

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

    public Inventory Inventory
    {
        get { return inventory; }
    }

    public Saloon()
    {
        abilities.Add(new TrainWorker(this));
        abilities.Add(new TrainMerchant(this));
    }

    [Command]
    private void CmdTrainWorker()
    {
        if (isActiveAndEnabled)
        {
            //TODO: Add training cost (e.g. 20 food)
            //TODO: Add training queue and time delay for spawning (to be done in RtsTrainingBuilding)
            //TODO: Add colision detection with other entities
            FindObjectOfType<EntityControl>().SpawnEntity(workerPrefab, spawnPoint.transform.position, Client);
        }
    }

    [Command]
    private void CmdTrainMerchant()
    {
        if (isActiveAndEnabled)
        {
            //TODO: Add training cost (e.g. 20 food)
            //TODO: Add training queue and time delay for spawning (to be done in RtsTrainingBuilding)
            //TODO: Add colision detection with other entities
            FindObjectOfType<EntityControl>().SpawnEntity(merchantPrefab, spawnPoint.transform.position, Client);
        }
    }

    private class TrainWorker : AbilityBase
    {
        private Saloon saloon;
        public TrainWorker(Saloon saloon) : base("Worker", "Train a worker, which can build structures and harvest resources.", KeyCode.W, "TrainWorker")
        {
            this.saloon = saloon;
        }

        public override bool CanExecute
        {
            get { return true; } //TODO: Check for resources and queue state.
        }

        public override void Execute()
        {
            if (saloon.hasAuthority) { saloon.CmdTrainWorker(); }
        }
    }

    private class TrainMerchant : AbilityBase
    {
        private Saloon saloon;
        public TrainMerchant(Saloon saloon) : base("Merchant", "Train a merchant, which can transport resources between storage buildings.", KeyCode.E, "TrainWorker")
        {
            this.saloon = saloon;
        }

        public override bool CanExecute
        {
            get { return true; } //TODO: Check for resources and queue state.
        }

        public override void Execute()
        {
            if (saloon.hasAuthority) { saloon.CmdTrainMerchant(); }
        }
    }
}
