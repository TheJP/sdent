using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Networking;

public class Worker : RtsUnit, IHasInventory
{
    public const int InventorySize = 20;
    public const float WorkDistance = 12;
    public const float WorkerBuildingSpeed = 100f;
    public const float GatheringTime = 0.1f;

    //Used vor collision avoidance, when constructing buildings
    public const float BuildingSpace = 12f;
    public const float UnitSpace = 7f;

    public GameObject cantBuildHere;

    private enum States { Idle, Traveling, Building, Gathering, FetchingResources }

    /// <summary>Building or Resource, which this worker is assigned to. This is not null only for the client with authority.</summary>
    private RtsEntity assignedWork = null;
    private States workerState = States.Idle;
    private float lastGatheredTime = 0f;
    private readonly IAbility moveAbility;
    private readonly Resume resumeAbility;
    private readonly AbilityWithTarget work;
    private readonly Inventory inventory = new Inventory(InventorySize);

    public override IAbility RightClickAbility
    {
        get { return moveAbility; }
    }

    public override AbilityWithTarget RightClickWithTargetAbility
    {
        get { return work; }
    }

    public Inventory Inventory
    {
        get { return inventory; }
    }

    private EntityControl EntityControl
    {
        get { return FindObjectOfType<EntityControl>(); }
    }

    public Worker()
    {
        foreach (var ability in Abilities.ToList()) { RemoveAbility(ability); }
        resumeAbility = new Resume(new ResumeAbility(this), this);
        moveAbility = new NewOrder(new MoveAbility(this), this, resumeAbility);
        AddAbility(moveAbility);
        AddAbility(new Stop(new StopAbility(this), this, resumeAbility));
        AddAbility(resumeAbility);
        work = new Work(KeyCode.W, this);
        AddAbility(work);
        AddAbility(new NewOrder(new BuildBuilding("Storage House", "Build a storage house, where workers can load off their resources.", KeyCode.Q, this, Buildings.StorageHouse), this, resumeAbility));
        AddAbility(new NewOrder(new BuildBuilding("Farmhouse", "Build a farm house, which produces food.", KeyCode.F, this, Buildings.FarmHouse), this, resumeAbility));
        AddAbility(new NewOrder(new BuildBuilding("Pottery", "Build a pottery, which produces bricks.", KeyCode.E, this, Buildings.Pottery), this, resumeAbility));
        AddAbility(new NewOrder(new BuildBuilding("Stable", "Build a stable, where riding units can be trained.", KeyCode.R, this, Buildings.Stable), this, resumeAbility));
        AddAbility(new NewOrder(new BuildBuilding("Workshop", "Build a workshop, which produces planks.", KeyCode.V, this, Buildings.Workshop), this, resumeAbility));
        AddAbility(new NewOrder(new BuildBuilding("Smeltery", "Build a smeltery, which produces steel.", KeyCode.G, this, Buildings.Smeltery), this, resumeAbility));
    }

    private RtsBuilding FindNearestStorage()
    {
        return Utility.FindNearestStorage(FindObjectOfType<EntityControl>().Entities, transform.position);
    }

    /// <summary>Stop the current work and switch to being Idle.</summary>
    private void StopWork()
    {
        if (!hasAuthority) { return; }
        if (assignedWork != null)
        {
            if (assignedWork is ConstructionSite) { (assignedWork as ConstructionSite).WorkerStopBuilding(this); }
        }
        workerState = States.Idle;
        assignedWork = null;
    }

    [ClientRpc]
    public void RpcAssignWork(GameObject entity)
    {
        if (hasAuthority)
        {
            var work = entity.GetComponent<RtsEntity>();
            assignedWork = entity.GetComponent<RtsEntity>();
            //Set work for every selected worker
            foreach(var worker in FindObjectOfType<EntityControl>().SelectedEntities.Get<Worker>().Where(w => w.hasAuthority))
            {
                worker.StopWork();
                worker.assignedWork = work;
                worker.resumeAbility.Resumeable = false;
            }
        }
    }

    [Client]
    public void FinishedBuilding()
    {
        if (workerState == States.Building)
        {
            assignedWork = null;
            workerState = States.Idle;
        }
    }

    /// <summary>Load all resources from the worker to the given inventory</summary>
    /// <param name="storageInventory"></param>
    [Client]
    private void Loadoff(Inventory storageInventory)
    {
        foreach (var resource in Inventory.ToList())
        {
            if (storageInventory.AddResources(resource.Key, resource.Value)) { Inventory.RemoveResources(resource.Key, resource.Value); }
        }
    }

    [Client]
    private void FetchingResourcesForConstruction(NavMeshAgent agent)
    {
        var constructionSite = assignedWork as ConstructionSite;
        var nearestStorage = FindNearestStorage();
        if ((nearestStorage.transform.position - transform.position).sqrMagnitude > WorkDistance * WorkDistance)
        {
            //Travel to storage house
            agent.SetDestination(nearestStorage.transform.position);
            workerState = States.Traveling;
        }
        else
        {
            //Load needed resources
            var target = (nearestStorage as IHasInventory).Inventory;
            Loadoff(target);
            var random = new System.Random();
            foreach (var need in constructionSite.NeededResources.OrderBy(n => random.NextDouble()))
            {
                var amount = Math.Min(target[need.Key], Math.Min(need.Value - constructionSite.Inventory[need.Key], Inventory.FreeSpace));
                if(amount > 0 && target.RemoveResources(need.Key, amount))
                {
                    Inventory.AddResources(need.Key, amount);
                }
            }
            //Travel to work
            agent.SetDestination(assignedWork.transform.position);
            workerState = States.FetchingResources;
        }
    }

    [Client]
    private void DoAssignedWork(NavMeshAgent agent)
    {
        if (assignedWork is RtsResource && Inventory.Count() >= InventorySize)
        {
            var nearesStorage = FindNearestStorage();
            if(nearesStorage == null) { return; }
            if((nearesStorage.transform.position - transform.position).sqrMagnitude <= WorkDistance * WorkDistance)
            {
                //Load off resource
                Loadoff((nearesStorage as IHasInventory).Inventory);
                //Travel back to resource
                if (Inventory.Count() < InventorySize && agent.SetDestination(assignedWork.transform.position))
                {
                    agent.Resume();
                    workerState = States.Traveling;
                }
            }
            else
            {
                //Travel to storage house
                if (agent.SetDestination(nearesStorage.transform.position))
                {
                    agent.Resume();
                    workerState = States.Traveling;
                }
            }
        }
        else if(assignedWork is ConstructionSite && (assignedWork as ConstructionSite).Inventory.FreeSpace > 0)
        {
            FetchingResourcesForConstruction(agent);
        }
        else if ((assignedWork.transform.position - transform.position).sqrMagnitude <= WorkDistance * WorkDistance)
        {
            //Arrived at travel target
            if (assignedWork is ConstructionSite)
            {
                //Build building
                (assignedWork as ConstructionSite).WorkerStartBuilding(this);
                workerState = States.Building;
            }
            else if(assignedWork is RtsResource)
            {
                //Gather resource
                workerState = States.Gathering;
            }
            else { workerState = States.Idle; }
        }
        else if(workerState != States.Traveling)
        {
            if (agent.SetDestination(assignedWork.transform.position))
            {
                agent.Resume();
                workerState = States.Traveling;
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        var agent = GetComponent<NavMeshAgent>();
        if (hasAuthority)
        {
            switch (workerState)
            {
                case States.Idle:
                    if (assignedWork != null) { DoAssignedWork(agent); }
                    break;
                case States.Traveling:
                    if (!agent.pathPending && (agent.destination - transform.position).sqrMagnitude <= WorkDistance * WorkDistance)
                    {
                        //Completed path: transition to work / idle
                        if (assignedWork == null) { workerState = States.Idle; }
                        else { DoAssignedWork(agent); }
                    }
                    break;
                case States.Building:
                    if (assignedWork == null) { workerState = States.Idle; }
                    break;
                case States.Gathering:
                    if (assignedWork == null) { workerState = States.Idle; }
                    else if(Inventory.Count() >= InventorySize)
                    {
                        //Go home
                        var nearestStorage = FindNearestStorage();
                        if (nearestStorage != null && agent.SetDestination(nearestStorage.transform.position))
                        {
                            agent.Resume();
                            workerState = States.Traveling;
                        }
                    }
                    else if(lastGatheredTime + GatheringTime < Time.time)
                    {
                        lastGatheredTime = Time.time;
                        Inventory.AddResources((assignedWork as RtsResource).ResourceType, 1);
                        (assignedWork as RtsResource).CmdTakeResource(1);
                    }
                    break;
                case States.FetchingResources:
                    if(assignedWork == null) { workerState = States.Idle; }
                    else if((assignedWork.transform.position - transform.position).sqrMagnitude <= WorkDistance * WorkDistance)
                    {
                        Loadoff((assignedWork as IHasInventory).Inventory);
                        if ((assignedWork as ConstructionSite).Inventory.FreeSpace > 0)
                        {
                            agent.SetDestination(FindNearestStorage().transform.position);
                            workerState = States.Traveling;
                        }
                        else { DoAssignedWork(agent); }
                    }
                    break;
            }
        }
    }

    [Command]
    private void CmdBuildBuilding(Buildings building, Vector3 position)
    {
        var entityControl = FindObjectOfType<EntityControl>();
        var collision = entityControl.Entities.Any(entity => (entity is RtsUnit) ?
            //Checks for unit
            position.x >= entity.transform.position.x - UnitSpace &&
            position.x <= entity.transform.position.x + UnitSpace &&
            position.z >= entity.transform.position.z - UnitSpace &&
            position.z <= entity.transform.position.z + UnitSpace :
            //Checks for buildings
            position.x >= entity.transform.position.x - BuildingSpace &&
            position.x <= entity.transform.position.x + BuildingSpace &&
            position.z >= entity.transform.position.z - BuildingSpace &&
            position.z <= entity.transform.position.z + BuildingSpace);
        if (!collision) { entityControl.BuildConstructionSite(building, position, Client, gameObject); }
        else { RpcCantBuildHere(position); }
    }

    [ClientRpc]
    private void RpcCantBuildHere(Vector3 position)
    {
        if (hasAuthority) { Instantiate(cantBuildHere, position, Quaternion.identity); }
    }

    private class BuildBuilding : AbilityBase
    {
        private Worker worker;
        private Buildings finalBuilding;

        public BuildBuilding(string name, string lore, KeyCode key, Worker worker, Buildings finalBuilding) : base(name, lore, key, "BuildBuilding", isSettingTarget: true)
        {
            this.worker = worker;
            this.finalBuilding = finalBuilding;
        }

        public override void Execute()
        {
            if (worker.hasAuthority)
            {
                worker.EntityControl.StartTargeting(ClickedTarget, "Select building location...");
            }
        }

        private bool ClickedTarget(IEnumerable<RaycastHit> hits)
        {
            //Only execute this for one worker
            var ground = Utility.GetGroundFromHits(hits);
            if (ground.HasValue) { worker.CmdBuildBuilding(finalBuilding, ground.Value); return true; }
            else { worker.EntityControl.ShowHintText("Select valid building location..."); return false; }
        }
    }

    private class Work : AbilityWithTarget
    {
        private readonly Worker worker;
        public Work(KeyCode key, Worker worker) : base("Work", "Work at target. This can be: 'Building at a construction site' or 'Gathering resources'", key, "BuildBuilding")
        {
            this.worker = worker;
        }

        public override bool IsSettingTarget
        {
            get { return true; }
        }

        public override void Execute()
        {
            if (worker.hasAuthority) { worker.EntityControl.StartTargeting(OnClickTarget, "Select working place..."); }
        }

        private bool OnClickTarget(IEnumerable<RaycastHit> hits)
        {
            var target = Utility.GetRtsEntityFromHits(hits);
            if (!(target is RtsResource || target is ConstructionSite))
            {
                worker.EntityControl.ShowHintText("Please select a construction site or a resource...");
                return false;
            }
            if(target is ConstructionSite && !target.hasAuthority)
            {
                worker.EntityControl.ShowHintText("You can't work on the enemies construction site... Select working place...");
                return false;
            }
            foreach(var worker in this.worker.EntityControl.SelectedEntities.Get<Worker>().Where(w => w.hasAuthority))
            {
                ExecuteForWorker(worker, target);
            }
            return true;
        }

        public override void Execute(RtsEntity target)
        {
            if (!worker.hasAuthority || target == null || (target is ConstructionSite && !target.hasAuthority)) { return; }
            if (target is ConstructionSite || target is RtsResource){ ExecuteForWorker(worker, target); }
        }

        private void ExecuteForWorker(Worker worker, RtsEntity target)
        {
            worker.StopWork();
            worker.resumeAbility.Resumeable = false;
            worker.GetComponent<NavMeshAgent>().SetDestination(worker.transform.position);
            worker.assignedWork = target;
        }
    }

    private class Resume : AbilityDecorator
    {
        private readonly Worker worker;
        public RtsEntity PreviousAssignedWork { get; set; }
        public States PreviousWorkerState { get; set; }
        public  bool Resumeable { get; set; }
        public Resume(IAbility decorated, Worker worker) : base(decorated)
        {
            this.worker = worker;
            Resumeable = false;
        }
        public override void Execute()
        {
            if (!worker.hasAuthority) { return; }
            base.Execute();
            if (Resumeable)
            {
                worker.workerState = PreviousWorkerState;
                worker.assignedWork = PreviousAssignedWork;
                if(PreviousAssignedWork != null)
                {
                    if (PreviousAssignedWork is ConstructionSite) { (PreviousAssignedWork as ConstructionSite).WorkerStartBuilding(worker); }
                }
            }
        }
    }

    private class Stop : AbilityDecorator
    {
        private readonly Worker worker;
        private readonly Resume resumeAbility;
        public Stop(IAbility decorated, Worker worker, Resume resumeAbility) : base(decorated)
        {
            this.worker = worker;
            this.resumeAbility = resumeAbility;
        }

        public override void Execute()
        {
            if (!worker.hasAuthority) { return; }
            base.Execute();
            resumeAbility.PreviousAssignedWork = worker.assignedWork;
            resumeAbility.PreviousWorkerState = worker.workerState;
            resumeAbility.Resumeable = true;
            worker.StopWork();
        }
    }

    private class NewOrder : AbilityDecorator
    {
        private readonly Worker worker;
        private readonly Resume resumeAbility;
        public NewOrder(IAbility decorated, Worker worker, Resume resumeAbility) : base(decorated)
        {
            this.worker = worker;
            this.resumeAbility = resumeAbility;
        }

        public override void Execute()
        {
            if (!worker.hasAuthority) { return; }
            worker.StopWork();
            base.Execute();
            resumeAbility.Resumeable = false;
        }
    }
}
