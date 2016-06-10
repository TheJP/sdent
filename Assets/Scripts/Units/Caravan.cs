using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class Caravan : RtsUnit
{
    public GameObject saloonPrefab;

    protected override NetworkConnection Client
    {
        get { return connectionToClient; }
        set { throw new InvalidOperationException(); }
    }

    public Caravan()
    {
        AddAbility(new BuildSaloon(this));
    }

    protected override void Start()
    {
        base.Start();
        if (hasAuthority) { FindObjectOfType<CameraControl>().SetInitialPosition(this); }
    }

    [Command]
    private void CmdBuildSaloon()
    {
        if (isActiveAndEnabled)
        {
            FindObjectOfType<EntityControl>().SpawnEntity(saloonPrefab, transform.position, connectionToClient);
            CmdDie();
        }
    }

    /// <summary>Ability, which transforms the caravan into a saloon.</summary>
    private class BuildSaloon : AbilityBase
    {
        private Caravan caravan;
        public BuildSaloon(Caravan caravan) : base("Build Saloon", "Builds a saloon at the current location of the caravan. This will be the start of a new city.", KeyCode.Q)
        {
            this.caravan = caravan;
        }
        public override void Execute()
        {
            if (caravan.hasAuthority) { caravan.CmdBuildSaloon(); }
        }
    }
}
