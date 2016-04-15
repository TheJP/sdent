using UnityEngine;
using System.Collections;

public class Caravan : RtsUnit
{

    public MeshRenderer unitColorMesh;

    public Caravan()
    {
        SelectionChanged += CaravanSelectionChanged;
    }

    private void CaravanSelectionChanged()
    {
        unitColorMesh.material.color = Selected ? Color.green : Color.white;
    }
}
