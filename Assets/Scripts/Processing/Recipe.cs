using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>Represents a crafting recipe: This turns some material into some other material.</summary>
public class Recipe
{
    /// <summary>Time in ms, which this recipe takes to being crafted.</summary>
    public int CraftingTime { get; private set; }

    public IEnumerable<ResourceTuple> Input { get; private set; }

    public IEnumerable<ResourceTuple> Output { get; private set; }

    /// <summary>Creates a new recipe from the given parameters.</summary>
    /// <param name="craftingTime">Time in ms, which this recipe takes to being crafted.</param>
    public Recipe(int craftingTime, IEnumerable<ResourceTuple> input, IEnumerable<ResourceTuple> output)
    {
        this.CraftingTime = craftingTime;
        this.Input = input;
        this.Output = output;
    }
}

public class ResourceTuple
{
    public ResourceTypes Resource { get; private set; }
    public int Amount { get; private set; }
    public ResourceTuple(ResourceTypes resource, int amount)
    {
        this.Resource = resource;
        this.Amount = amount;
    }
}

public static class ResourceTypesExtensions
{
    public static ResourceTuple Times(this ResourceTypes resource, int amount)
    {
        return new ResourceTuple(resource, amount);
    }
}
