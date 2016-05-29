using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FilteredInventory : Inventory
{
    /// <summary>Creates an inventory with the given size.</summary>
    /// <param name="spaceAvailable">Size of the inventory</param>
    public FilteredInventory(int spaceAvailable, IEnumerable<Recipe> craftingFilter) : base(spaceAvailable)
    {
        //TODO
    }
}
