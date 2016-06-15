using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class RailStraight : RtsBuilding
{
    public override Buildings Type
    {
        get { return Buildings.RailStraight; }
    }

    public override IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return new[] { ResourceTypes.Steel.Times(200), ResourceTypes.Plank.Times(200) }; }
    }
}
