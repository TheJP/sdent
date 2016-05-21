using UnityEngine;
using System.Collections;

public class Caravan : RtsUnit
{
    public MeshRenderer unitColorMesh;

    protected override void Start()
    {
        base.Start();
        if (hasAuthority) { FindObjectOfType<CameraControl>().SetInitialPosition(this); }
    }

    public Caravan()
    {
        SelectionChanged += CaravanSelectionChanged;
    }

    private void CaravanSelectionChanged()
    {
        unitColorMesh.material.color = Selected ? Color.green : Color.white;
    }
}
