using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Pottery : RtsCraftingBuilding
{
    public const float CraftingTime = 4f;

    private Recipe[] recipes = new[]
    {
        new Recipe(CraftingTime, new[] { ResourceTypes.Clay.Times(20), ResourceTypes.Coal.Times(10) }, new[] { ResourceTypes.Brick.Times(10) })
    };
    public override IEnumerable<Recipe> Recipes
    {
        get { return recipes; }
    }

    public override Buildings Type
    {
        get { return Buildings.Pottery; }
    }

    public override IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return new[] { ResourceTypes.Clay.Times(100), ResourceTypes.Plank.Times(100) }; }
    }
}
