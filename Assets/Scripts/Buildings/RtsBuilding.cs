using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class RtsBuilding : RtsEntity
{
    public abstract Buildings Type { get; }

    public virtual IEnumerable<ResourceTuple> BuildingCosts
    {
        get { return Enumerable.Empty<ResourceTuple>(); }
    }
}
