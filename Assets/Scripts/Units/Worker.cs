using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Networking;

public class Worker : RtsUnit
{
    public const float WorkDistance = 12;
    public const float WorkerBuildingSpeed = 1f;

    private enum States { Idle, Traveling, Building }

    /// <summary>Building or Resource, which this worker is assigned to. This is not null only for the client with authority.</summary>
    private RtsEntity assignedWork = null;
    private States workerState = States.Idle;
    private readonly IAbility moveAbility;

    public override IAbility RightClickAbility
    {
        get { return moveAbility; }
    }

    public Worker()
    {
        foreach (var ability in Abilities.ToList()) { RemoveAbility(ability); }
        var resume = new Resume(new ResumeAbility(this), this);
        moveAbility = new NewOrder(new MoveAbility(this), this, resume);
        AddAbility(moveAbility);
        AddAbility(new Stop(new StopAbility(this), this, resume));
        AddAbility(resume);
        AddAbility(new NewOrder(new BuildBuilding("Storage House", "Build a storage house, where workers can load off their resources.", KeyCode.Q, this, Buildings.StorageHouse), this, resume));
        AddAbility(new NewOrder(new BuildBuilding("Stable", "Build a stable, where riding units can be trained.", KeyCode.W, this, Buildings.Stable), this, resume));
    }

    [ClientRpc]
    public void RpcAssignWork(GameObject entity)
    {
        if (hasAuthority) { assignedWork = entity.GetComponent<RtsEntity>(); }
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

    [Client]
    private void DoAssignedWork(NavMeshAgent agent)
    {
        if ((assignedWork.transform.position - transform.position).sqrMagnitude <= WorkDistance * WorkDistance)
        {
            if (assignedWork is ConstructionSite)
            {
                (assignedWork as ConstructionSite).WorkerStartBuilding(this);
                workerState = States.Building;
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
            //TODO: Get resources for the building first
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
            }
        }
    }

    [Command]
    private void CmdBuildBuilding(Buildings building)
    {
        var ground = Utility.RayMouseToGround();
        if (ground.HasValue)
        {
            //TODO: entity avoidance for new buildings
            FindObjectOfType<EntityControl>().BuildConstructionSite(building, ground.Value, Client, gameObject);
        }
    }

    private class BuildBuilding : AbilityBase
    {
        private Worker worker;
        private Buildings finalBuilding;

        public BuildBuilding(string name, string lore, KeyCode key, Worker worker, Buildings finalBuilding) : base(name, lore, key, "BuildBuilding")
        {
            this.worker = worker;
            this.finalBuilding = finalBuilding;
        }

        public override void Execute()
        {
            if (worker.hasAuthority) { worker.CmdBuildBuilding(finalBuilding); }
        }
    }

    private class Resume : AbilityDecorator
    {
        private readonly Worker worker;
        public RtsEntity PreviousAssignedWork { get; set; }
        public States PreviousWorkerState { get; set; }
        public  bool ResumeAble { get; set; }
        public Resume(IAbility decorated, Worker worker) : base(decorated)
        {
            this.worker = worker;
            ResumeAble = false;
        }
        public override void Execute()
        {
            base.Execute();
            if (ResumeAble)
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
            base.Execute();
            if(worker.assignedWork != null)
            {
                if (worker.assignedWork is ConstructionSite) { (worker.assignedWork as ConstructionSite).WorkerStopBuilding(worker); }
            }
            resumeAbility.PreviousAssignedWork = worker.assignedWork;
            worker.assignedWork = null;
            resumeAbility.PreviousWorkerState = worker.workerState;
            worker.workerState = States.Idle;
            resumeAbility.ResumeAble = true;
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
            if (worker.assignedWork != null)
            {
                if (worker.assignedWork is ConstructionSite) { (worker.assignedWork as ConstructionSite).WorkerStopBuilding(worker); }
            }
            worker.workerState = States.Idle;
            worker.assignedWork = null;
            base.Execute();
            resumeAbility.ResumeAble = false;
        }
    }
}
