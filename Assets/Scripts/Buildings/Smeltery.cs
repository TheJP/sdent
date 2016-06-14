using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Smeltery : RtsCraftingBuilding
{
    public const float CraftingTime = 10f;

    private Recipe[] recipes = new[]
    {
        new Recipe(CraftingTime, new[] { ResourceTypes.Iron.Times(20), ResourceTypes.Coal.Times(20) }, new[] { ResourceTypes.Steel.Times(10) })
    };
    public override IEnumerable<Recipe> Recipes
    {
        get { return recipes; }
    }

    public override Buildings Type
    {
        get { return Buildings.Smeltery; }
    }

    public override IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return new[] { ResourceTypes.Brick.Times(100), ResourceTypes.Iron.Times(100), ResourceTypes.Plank.Times(100) }; }
    }
}
