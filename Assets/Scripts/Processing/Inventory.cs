using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Inventory : IEnumerable<KeyValuePair<ResourceTypes, int>>
{
    private readonly Dictionary<ResourceTypes, int> inventory = new Dictionary<ResourceTypes, int>();
    private readonly int spaceAvailable;
    private int spaceTaken = 0;

    public int this[ResourceTypes resource]
    {
        get
        {
            if (!inventory.ContainsKey(resource)) { return 0; }
            return inventory[resource];
        }
    }

    public int SpaceAvailable { get { return spaceAvailable; } }

    /// <summary>Creates an inventory with the given size.</summary>
    /// <param name="spaceAvailable">Size of the inventory</param>
    public Inventory(int spaceAvailable)
    {
        this.spaceAvailable = spaceAvailable;
    }

    public int Count() { return spaceTaken; }

    /// <summary>Determines, if all the resources can be added.</summary>
    /// <param name="resources"></param>
    /// <returns>True if those resources can be added.</returns>
    public  virtual bool CanAddResources(IEnumerable<ResourceTuple> resources)
    {
        return resources.Sum(tuple => Math.Abs(tuple.Amount)) + spaceTaken <= SpaceAvailable;
    }

    public virtual bool AddResources(ResourceTypes resource, int amount)
    {
        if(amount < 0) { return RemoveResources(resource, -amount); }
        if (spaceTaken + amount > spaceAvailable) { return false; }
        spaceTaken += amount;
        if (inventory.ContainsKey(resource)) { inventory[resource] += amount; }
        else { inventory[resource] = amount; }
        return true;
    }

    public virtual bool RemoveResources(ResourceTypes resource, int amount)
    {
        if (amount < 0) { return AddResources(resource, -amount); }
        if (!inventory.ContainsKey(resource) || inventory[resource] < amount) { return false; }
        spaceTaken -= amount;
        inventory[resource] -= amount;
        return true;
    }

    public IEnumerator<KeyValuePair<ResourceTypes, int>> GetEnumerator() { return inventory.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return inventory.GetEnumerator(); }
}
