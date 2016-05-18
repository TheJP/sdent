using UnityEngine;
using System.Collections;
using System;

public class GoldMine : RtsResource
{
    public override ResourceTypes ResourceType
    {
        get { return ResourceTypes.Gold; }
    }
    public override float MaxState
    {
        get { return 5000f; }
    }
}
