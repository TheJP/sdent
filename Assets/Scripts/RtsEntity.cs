﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public abstract class RtsEntity : NetworkBehaviour
{
    private bool selected = false; //This is intentionally not a synced variable

    [SyncVar]
    public float state;

    public Slider barSlider;
    public Image barFillImage;
    public Texture portraitImage;
    public MeshRenderer entityColorMesh;

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

    /// <summary>Abilities, which can be executed for this <see cref="RtsEntity"/>.</summary>
    public virtual IEnumerable<IAbility> Abilities
    {
        get { return Enumerable.Empty<IAbility>(); }
    }

    protected virtual NetworkConnection Client { get; set; }

    [Server]
    public NetworkConnection GetClient() { return Client; }

    [Server]
    public void SetClient(NetworkConnection value) { Client = value; }

    /// <summary>
    /// Triggers, when the selection of this rts entity changes.
    /// </summary>
    public event System.Action SelectionChanged;

    /// <summary>
    /// Triggers, when the rts entity dies.
    /// </summary>
    public event System.Action<RtsEntity> EntityDied;

    public RtsEntity()
    {
        SelectionChanged += EntitySelectionChanged;
    }

    private void EntitySelectionChanged()
    {
        if (isActiveAndEnabled && entityColorMesh != null) { entityColorMesh.material.color = Selected ? Color.green : Color.white; }
    }

    [Command]
    protected void CmdDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (EntityDied != null) { EntityDied(this); }
    }

    /// <summary>
    /// Called, when the local player right clicks with this entity selected.
    /// </summary>
    public virtual IAbility RightClickAbility
    {
        get { return null; }
    }

    /// <summary>
    /// Called, when the local player right clicks a target with this entity selected.
    /// </summary>
    public virtual AbilityWithTarget RightClickWithTargetAbility
    {
        get { return null; }
    }

    protected virtual void Start()
    {
        state = MaxState; //Set initial state on server and clients (lag compensation)
        FindObjectOfType<EntityControl>().AddEntity(this);
    }

    protected virtual void Update()
    {
        if (barSlider != null && barFillImage != null)
        {
            var relativeState = state / MaxState;
            barSlider.value = relativeState;
            barFillImage.color = Color.Lerp(emptyColor, fullColor, relativeState);
        }
    }
}
