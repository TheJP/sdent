using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class Caravan : RtsUnit
{
    public MeshRenderer unitColorMesh;
    public GameObject saloonPrefab;

    private readonly List<IAbility> abilities = new List<IAbility>();
    public override IEnumerable<IAbility> Abilities
    {
        get { return abilities; }
    }

    public Caravan()
    {
        SelectionChanged += CaravanSelectionChanged;
        abilities.Add(new BuildSaloon(this));
    }

    protected override void Start()
    {
        base.Start();
        if (hasAuthority) { FindObjectOfType<CameraControl>().SetInitialPosition(this); }
    }

    private void CaravanSelectionChanged()
    {
        unitColorMesh.material.color = Selected ? Color.green : Color.white;
    }

    [Command]
    private void CmdBuildSaloon()
    {
        if (isActiveAndEnabled)
        {
            FindObjectOfType<EntityControl>().BuildBuilding(saloonPrefab, transform.position, connectionToClient);
            CmdDie();
        }
    }

    /// <summary>Ability, which transforms the caravan into a saloon.</summary>
    private class BuildSaloon : AbilityBase
    {
        private Caravan caravan;
        public BuildSaloon(Caravan caravan) : base("Build Saloon", "Builds a saloon at the current location of the caravan. This will be the start of a new city.")
        {
            this.caravan = caravan;
        }
        public override void Execute()
        {
            if (caravan.hasAuthority) { caravan.CmdBuildSaloon(); }
        }
    }
}
