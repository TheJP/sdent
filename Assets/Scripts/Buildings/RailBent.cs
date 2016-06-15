using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RailBent : RtsBuilding
{
    public override Buildings Type
    {
        get { return Buildings.RailBent; }
    }

    public override IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return new[] { ResourceTypes.Steel.Times(200), ResourceTypes.Plank.Times(200) }; }
    }
}
