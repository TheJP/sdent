﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>Base class for every building, which can craft certain recipes.</summary>
public abstract class RtsCraftingBuilding : RtsBuilding, IHasInventory
{
    /// <summary>Factor, how much more space a crafting building has, than it needs for one recipe (including output).</summary>
    public const int CraftingSpaceFactor = 2;

    public abstract IEnumerable<Recipe> Recipes { get; }

    public Inventory Inventory { get; private set; }

    public RtsCraftingBuilding()
    {
        var exactSpace = Recipes.SelectMany(r => r.Input.Union(r.Output)).Sum(tuple => tuple.Amount);
        this.Inventory = new FilteredInventory(CraftingSpaceFactor * exactSpace, Recipes);
    }
}
