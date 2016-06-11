using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MoveAbility : AbilityBase
{
    private readonly RtsEntity entity;

    public MoveAbility(RtsEntity entity) : base("Move", "Move this unit to the target location", KeyCode.M)
    {
        this.entity = entity;
    }

    public override void Execute()
    {
        if (!entity.hasAuthority) { return; }
        //Cast a ray to determine where to move
        var hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
        var ground = hits.Where(hit => hit.transform.tag == "Ground").Select(hit => (Vector3?)hit.point).FirstOrDefault();
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

    public StopAbility(RtsEntity entity) : base("Stop Unit", "Stops the current activity of this unit", KeyCode.S)
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

    public ResumeAbility(RtsEntity entity) : base("Resume", "Unit that was stopped will resume on the same path", KeyCode.D)
    {
        this.entity = entity;
    }

    public override void Execute()
    {
        if (entity.hasAuthority) { entity.GetComponent<NavMeshAgent>().Resume(); }
    }
}
