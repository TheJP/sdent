using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class RtsUnit : RtsEntity
{
    private readonly List<IAbility> abilities;
    public override IEnumerable<IAbility> Abilities
    {
        get { return abilities.AsReadOnly(); }
    }

    protected void AddAbility(IAbility ability)
    {
        abilities.Add(ability);
    }

    public RtsUnit()
    {
        abilities = new List<IAbility>()
        {
            new MoveAbility(this),
            new StopAbility(this),
            new ResumeAbility(this)
        };
    }
}
