using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Inventory
{
    private readonly Dictionary<ResourceTypes, int> inventory = new Dictionary<ResourceTypes, int>();
    private readonly int spaceAvailable;
    private int spaceTaken = 0;

    /// <summary>Creates an inventory with the given size.</summary>
    /// <param name="spaceAvailable">Size of the inventory</param>
    public Inventory(int spaceAvailable)
    {
        this.spaceAvailable = spaceAvailable;
    }

    public int this[ResourceTypes resource]
    {
        get
        {
            if (!inventory.ContainsKey(resource)) { return 0; }
            return inventory[resource];
        }
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
}
