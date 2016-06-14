using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Linq;
using Assets.Scripts.Utility;
using System.Collections.Generic;

public enum ResourceTypes
{
    // Name of the enum is the name of the icon
    Gold,
    Iron,
    Wood,
    Coal,
    Clay,
    Food
}

public abstract class RtsResource : RtsEntity
{
    public static IEnumerable<ResourceTypes> Resources
    {
        get { return (IEnumerable<ResourceTypes>)Enum.GetValues(typeof(ResourceTypes)); }
    }

    public abstract ResourceTypes ResourceType { get; }

    protected override NetworkConnection Client
    {
        get { return base.Client; }
        set { throw new InvalidOperationException(); }
    }
    public override float MaxState
    {
        get { return 99999f; }
    }

    [Command]
    public void CmdTakeResource(int amount)
    {
        if(amount <= 0) { return; }
        state -= amount;
    }
}
