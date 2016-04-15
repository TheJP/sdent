using UnityEngine;
using System.Collections;

public class Caravan : RtsUnit
{
    public Caravan()
    {
        SelectionChanged += CaravanSelectionChanged;
    }

    private void CaravanSelectionChanged()
    {
        GetComponent<MeshRenderer>().material.color = Selected ? Color.green : Color.white;
    }
}
