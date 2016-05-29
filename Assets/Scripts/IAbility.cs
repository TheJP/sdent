﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Ability of buildings and units like build, attack, produce, ...
/// </summary>
public interface IAbility
{
    /// <summary>Name of the ability, which can be displayed in the gui.</summary>
    string Name { get; }

    /// <summary>Lore (Description) of the ability, which can be siaplyed in the gui.</summary>
    string Lore { get; }

    /// <summary>Flag, which determines, if this ability can currently be executed.</summary>
    bool CanExecute { get; }

    /// <summary>Execute the ability.</summary>
    /// <remarks>If the ability needs a target or further input, this data should be acquired via calls to helper methods.</remarks>
    void Execute();
}

/// <summary>Base class, which makes it easier to inherit from <see cref="IAbility"/>.</summary>
public abstract class AbilityBase : IAbility
{
    private readonly string name;
    public virtual string Name
    {
        get { return name; }
    }

    private readonly string lore;
    public virtual string Lore
    {
        get { return lore; }
    }

    private readonly bool canExecute;
    public virtual bool CanExecute
    {
        get { return canExecute; }
    }

    public abstract void Execute();

    public AbilityBase(string name, string lore, bool canExecute = true)
    {
        this.name = name;
        this.lore = lore;
        this.canExecute = canExecute;
    }
}