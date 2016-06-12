using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Helper class, which can be used to create <see cref="IAbility"/> decorators.
/// </summary>
public abstract class AbilityDecorator : IAbility
{
    protected IAbility Decorated { get; private set; }

    public virtual string Name
    {
        get { return Decorated.Name; }
    }

    public virtual string Lore
    {
        get { return Decorated.Lore; }
    }

    public virtual KeyCode Key
    {
        get { return Decorated.Key; }
    }
    public virtual bool CanExecute
    {
        get { return Decorated.CanExecute; }
    }

    public Texture Icon
    {
        get { return Decorated.Icon; }
    }

    public AbilityDecorator(IAbility decorated)
    {
        this.Decorated = decorated;
    }

    public virtual void Execute()
    {
        Decorated.Execute();
    }
}
