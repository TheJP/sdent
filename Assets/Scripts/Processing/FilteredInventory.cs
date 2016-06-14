using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FilteredInventory : Inventory
{

    /// <summary>Contains the maximal amounts, which the the inventory can hold per resource.</summary>
    private readonly Dictionary<ResourceTypes, int> maximals;

    /// <summary>Creates an inventory with the given size.</summary>
    /// <param name="spaceAvailable">Size of the inventory</param>
    /// <param name="maximals">The maximal amounts, which the the inventory can hold per resource.</param>
    public FilteredInventory(int spaceAvailable, Dictionary<ResourceTypes, int> maximals) : base(spaceAvailable)
    {
        if(maximals == null) { throw new ArgumentNullException(); }
        this.maximals = maximals;
    }

    /// <summary>Creates an inventory with the given size.</summary>
    /// <param name="maximals">The maximal amounts, which the the inventory can hold per resource.</param>
    public FilteredInventory(Dictionary<ResourceTypes, int> maximals) : this(maximals.Sum(max => max.Value), maximals) { }

    /// <summary>Creates an inventory with the given size.</summary>
    /// <param name="spaceAvailable">Size of the inventory</param>
    public FilteredInventory(int spaceAvailable, IEnumerable<Recipe> craftingFilter) : base(spaceAvailable)
    {
        if(craftingFilter == null) { throw new ArgumentNullException(); }
        maximals = craftingFilter
            .SelectMany(recipe => recipe.Input.Union(recipe.Output))
            .GroupBy(tuple => tuple.Resource, tuple => tuple.Amount)
            .ToDictionary(group => group.Key, group => RtsCraftingBuilding.CraftingSpaceFactor * group.Sum());
    }

    public override bool CanAddResources(IEnumerable<ResourceTuple> resources)
    {
        foreach (var tuple in resources
            .GroupBy(tuple => tuple.Resource, tuple => tuple.Amount)
            .Select(group => group.Key.Times(group.Sum())))
        {
            if (!maximals.ContainsKey(tuple.Resource) || this[tuple.Resource] + tuple.Amount > maximals[tuple.Resource]) { return false; }
        }
        return base.CanAddResources(resources);
    }

    public override bool AddResources(ResourceTypes resource, int amount)
    {
        if (!maximals.ContainsKey(resource) || this[resource] + amount > maximals[resource]) { return false; }
        return base.AddResources(resource, amount);
    }
}
