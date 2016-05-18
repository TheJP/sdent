using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public enum ResourceTypes
{
    Gold, Iron, Wood
}

public abstract class RtsResource : RtsEntity
{
    public abstract ResourceTypes ResourceType { get; }
}
