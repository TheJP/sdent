using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Stable : RtsBuilding
{
    public override Buildings Type
    {
        get { return Buildings.Stable; }
    }

    public override IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return new[] { ResourceTypes.Wood.Times(200), ResourceTypes.Steel.Times(100) }; }
    }
}
