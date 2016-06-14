using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Farmhouse : RtsCraftingBuilding
{
    private readonly List<Recipe> recipes = new List<Recipe>()
    {
        new Recipe(1f, Enumerable.Empty<ResourceTuple>(), new [] { new ResourceTuple(ResourceTypes.Food, 1)})
    };

    public override IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return new[] { ResourceTypes.Wood.Times(100) }; }
    }

    public override IEnumerable<Recipe> Recipes
    {
        get { return recipes; }
    }

    public override Buildings Type
    {
        get { return Buildings.FarmHouse; }
    }
}
