using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Caravan : RtsUnit
{
    public MeshRenderer unitColorMesh;

    private readonly List<IAbility> abilities = new List<IAbility>()
    {
        new BuildSaloon()
    };
    public override IEnumerable<IAbility> Abilities
    {
        get { return abilities; }
    }

    protected override void Start()
    {
        base.Start();
        if (hasAuthority) { FindObjectOfType<CameraControl>().SetInitialPosition(this); }
    }

    public Caravan()
    {
        SelectionChanged += CaravanSelectionChanged;
    }

    private void CaravanSelectionChanged()
    {
        unitColorMesh.material.color = Selected ? Color.green : Color.white;
    }

    /// <summary>Ability, which transforms the caravan into a saloon.</summary>
    private class BuildSaloon : AbilityBase
    {
        public BuildSaloon() : base("Build Saloon", "Builds a saloon at the current location of the caravan. This will be the start of a new city.") { }
        public override void Execute()
        {
            Debug.Log("Build saloon");
        }
    }
}
