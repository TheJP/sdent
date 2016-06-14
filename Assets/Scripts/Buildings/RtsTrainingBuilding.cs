using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Base class for every building, which trains some kind of unit.</summary>
public abstract class RtsTrainingBuilding : RtsBuilding, IHasInventory
{
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    public abstract Inventory Inventory { get; }

    [Command]
    private void CmdTrainUnit(GameObject prefab)
    {
        if (!isActiveAndEnabled) { return; }
        //TODO: Add training queue and time delay for spawning (to be done in RtsTrainingBuilding)
        var point1 = Vector3.Min(spawnPoint1.transform.position, spawnPoint2.transform.position);
        var point2 = Vector3.Max(spawnPoint1.transform.position, spawnPoint2.transform.position);
        FindObjectOfType<EntityControl>().SpawnEntity(prefab, new Vector3(Random.Range(point1.x, point2.x), 0, Random.Range(point1.z, point2.z)), Client);
    }

    protected class TrainUnit : AbilityBase
    {
        private readonly RtsTrainingBuilding building;
        private readonly System.Func<RtsTrainingBuilding, GameObject> prefab;
        private readonly IEnumerable<ResourceTuple> costs;
        public TrainUnit(RtsTrainingBuilding building, System.Func<RtsTrainingBuilding, GameObject> prefab, IEnumerable<ResourceTuple> costs, string name, string lore, KeyCode key, string iconName)
            : base(name, lore, key, iconName)
        {
            this.building = building;
            this.prefab = prefab;
            this.costs = costs;
        }

        public override bool CanExecute
        {
            get
            {
                return !costs.Any(cost => building.Inventory[cost.Resource] < cost.Amount);
                //TODO: Check for queue state.
            }
        }

        public override void Execute()
        {
            if (building.hasAuthority)
            {
                foreach (var cost in costs)
                {
                    if(!building.Inventory.RemoveResources(cost.Resource, cost.Amount)) { return; }
                }
                building.CmdTrainUnit(prefab(building));
            }
        }
    }
}
