using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public abstract class RtsEntity : NetworkBehaviour
{
    private bool selected = false;
    public bool Selected
    {
        get { return selected; }
        set
        {
            if (selected != value)
            {
                selected = value;
                if (SelectionChanged != null) { SelectionChanged(); }
            }
        }
    }
    public event System.Action SelectionChanged;
}
