using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public abstract class RtsEntity : NetworkBehaviour
{
    private bool selected = false; //This is intentionally not a synced variable

    /// <summary>
    /// Determines, if the local player has selected this entity.
    /// </summary>
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

    /// <summary>
    /// Triggers, when the selection of this rts entity changes.
    /// </summary>
    public event System.Action SelectionChanged;

    /// <summary>
    /// Called, when the local player right clicks with this entity selected.
    /// </summary>
    public virtual void DoRightClickAction(Vector3 position) { }
}
