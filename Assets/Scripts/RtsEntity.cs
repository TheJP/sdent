using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public abstract class RtsEntity : NetworkBehaviour
{
    private bool selected = false; //This is intentionally not a synced variable

    [SyncVar]
    public float state;

    public Slider barSlider;
    public Image barFillImage;

    private readonly Color fullColor = Color.green;
    private readonly Color emptyColor = Color.red;

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
    /// The maximum health / Resource Level of this entity.
    /// </summary>
    public virtual float MaxState
    {
        get { return 100f; }
    }

    /// <summary>
    /// Triggers, when the selection of this rts entity changes.
    /// </summary>
    public event System.Action SelectionChanged;

    /// <summary>
    /// Called, when the local player right clicks with this entity selected.
    /// </summary>
    public virtual void DoRightClickAction(Vector3 position) { }

    protected virtual void Start()
    {
        state = MaxState; //Set initial state on server and clients (lag compensation)
    }

    protected virtual void Update()
    {
        var relativeState = state / MaxState;
        barSlider.value = relativeState;
        barFillImage.color = Color.Lerp(emptyColor, fullColor, relativeState);
    }
}
