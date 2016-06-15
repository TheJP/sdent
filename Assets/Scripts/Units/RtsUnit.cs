using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class RtsUnit : RtsEntity
{
    public abstract Units Type { get; }

    private readonly IAbility moveAbility;
    public override IAbility RightClickAbility
    {
        get { return moveAbility; }
    }

    private readonly List<IAbility> abilities;
    public override IEnumerable<IAbility> Abilities
    {
        get { return abilities.AsReadOnly(); }
    }

    protected void AddAbility(IAbility ability)
    {
        abilities.Add(ability);
    }

    protected bool RemoveAbility(IAbility ability)
    {
        return abilities.Remove(ability);
    }

    public RtsUnit()
    {
        moveAbility = new MoveAbility(this);
        abilities = new List<IAbility>()
        {
            moveAbility,
            new StopAbility(this),
            new ResumeAbility(this)
        };
    }
}
