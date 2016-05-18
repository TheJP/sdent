using UnityEngine;
using System.Collections;

public class Caravan : RtsUnit
{
    public MeshRenderer unitColorMesh;

    public override float Speed
    {
        get { return 3f * base.Speed; }
    }

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
