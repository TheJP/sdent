using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using Assets.Scripts.Utility;

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
    public abstract ResourceTypes ResourceType { get; }
    protected override NetworkConnection Client
    {
        get { return base.Client; }
        set { throw new InvalidOperationException(); }
    }
}
