using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public enum ResourceTypes
{
    Gold, Iron, Wood, Coal, Clay
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
