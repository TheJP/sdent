using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class RailStraight : RtsBuilding
{
    private readonly List<IAbility> abilities = new List<IAbility>();
    private bool builtOther = false;

    public override IEnumerable<IAbility> Abilities
    {
        get { return abilities; }
    }

    public override Buildings Type
    {
        get { return Buildings.RailStraight; }
    }

    public override IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return new[] { ResourceTypes.Steel.Times(200), ResourceTypes.Plank.Times(200) }; }
    }

    public RailStraight()
    {
        abilities.Add(new BuildRailAbility(this));
    }

    private class BuildRailAbility : AbilityBase
    {
        private readonly RailStraight rail;
        public BuildRailAbility(RailStraight rail) : base("Build Rail", "Starts the construction of a straight rail at the end of the current line.", KeyCode.R, "BuildRail")
        {
            this.rail = rail;
        }

        public override bool CanExecute
        {
            get { return !rail.builtOther; }
        }

        public override void Execute()
        {
            if (rail.hasAuthority && !rail.builtOther)
            {
                rail.builtOther = true;
                FindObjectOfType<EntityControl>().BuildConstructionSite(Buildings.RailStraight, new Vector3(rail.transform.position.x, 0f, rail.transform.position.z - 10f), rail.Client, null);
            }
        }
    }
}
