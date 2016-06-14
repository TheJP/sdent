using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Workshop : RtsCraftingBuilding
{
    public const float CraftingTime = 4f;

    private Recipe[] recipes = new[]
    {
        new Recipe(CraftingTime, new[] { ResourceTypes.Wood.Times(20) }, new[] { ResourceTypes.Plank.Times(20) })
    };
    public override IEnumerable<Recipe> Recipes
    {
        get { return recipes; }
    }

    public override Buildings Type
    {
        get { return Buildings.Workshop; }
    }

    public override IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return new[] { ResourceTypes.Wood.Times(200), ResourceTypes.Clay.Times(100) }; }
    }
}
