using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class AbilityWithTarget : AbilityBase
{
    public AbilityWithTarget(string name, string lore, KeyCode key, string iconName, bool canExecute = true) : base(name, lore, key, iconName, canExecute) { }
    public override void Execute()
    {
        Execute(Utility.RayMouseToRtsEntity());
    }
    public abstract void Execute(RtsEntity target);
}
