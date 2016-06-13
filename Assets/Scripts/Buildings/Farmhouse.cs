using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Farmhouse : RtsCraftingBuilding
{
    public override IEnumerable<Recipe> Recipes
    {
        get { return Enumerable.Empty<Recipe>(); }
    }

    public override Buildings Type
    {
        get { return Buildings.FarmHouse; }
    }
}
