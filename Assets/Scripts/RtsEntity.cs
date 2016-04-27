using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public abstract class RtsEntity : NetworkBehaviour
{
    private bool selected = false; //This is intentionally not a synced variable

    [SyncVar]
    public float health;

    public Slider healthBarSlider;
    public Image healthBarFillImage;

    private readonly Color liveColor = Color.green;
    private readonly Color deadColor = Color.red;

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
    /// The maximum health of this entity.
    /// </summary>
    public virtual float MaxHealth
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
        health = MaxHealth; //Set initial health on server and clients (lag compensation)
    }

    protected virtual void Update()
    {
        var relativeHealth = health / MaxHealth;
        healthBarSlider.value = relativeHealth;
        healthBarFillImage.color = Color.Lerp(deadColor, liveColor, relativeHealth);
    }
}
