using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MoveAbility : AbilityBase
{
    private readonly RtsEntity entity;

    public MoveAbility(RtsEntity entity) : base("Move", "Move this unit to the target location", KeyCode.M, "Move")
    {
        this.entity = entity;
    }

    public override void Execute()
    {
        if (!entity.hasAuthority) { return; }
        //Cast a ray to determine where to move
        var ground = Utility.RayMouseToGround();
        if (ground.HasValue)
        {
            //Move to location, where the ground was hit
            var agent = entity.GetComponent<NavMeshAgent>();
            agent.SetDestination(ground.Value);
            agent.Resume();
        }
    }
}

public class StopAbility : AbilityBase
{
    private readonly RtsEntity entity;

    public StopAbility(RtsEntity entity) : base("Stop Unit", "Stops the current activity of this unit", KeyCode.S, "Stop")
    {
        this.entity = entity;
    }

    public override void Execute()
    {
        if (entity.hasAuthority) { entity.GetComponent<NavMeshAgent>().Stop(); }
    }
}

public class ResumeAbility : AbilityBase
{
    private readonly RtsEntity entity;

    public ResumeAbility(RtsEntity entity) : base("Resume", "Unit that was stopped will resume on the same path", KeyCode.D, "Resume")
    {
        this.entity = entity;
    }

    public override void Execute()
    {
        if (entity.hasAuthority) { entity.GetComponent<NavMeshAgent>().Resume(); }
    }
}
