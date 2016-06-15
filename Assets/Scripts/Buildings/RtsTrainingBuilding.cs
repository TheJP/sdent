using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Base class for every building, which trains some kind of unit.</summary>
public abstract class RtsTrainingBuilding : RtsBuilding, IHasInventory
{
    /// <summary>Time in seconds, which it takes to train an unit.</summary>
    public const float TrainingTime = 5f;
    /// <summary>Maximal amount of units in the training queue.</summary>
    public const int TrainingQueueSize = 5;

    public Transform spawnPoint1;
    public Transform spawnPoint2;

    private readonly Queue<GameObject> trainingQueue = new Queue<GameObject>();
    private float lastTrained = 0;

    public abstract Inventory Inventory { get; }

    public IEnumerable<GameObject> TrainingQueue
    {
        get { return trainingQueue.ToList(); }
    }

    [Command]
    private void CmdTrainUnit(GameObject prefab)
    {
        if (!isActiveAndEnabled) { return; }
        var point1 = Vector3.Min(spawnPoint1.transform.position, spawnPoint2.transform.position);
        var point2 = Vector3.Max(spawnPoint1.transform.position, spawnPoint2.transform.position);
        FindObjectOfType<EntityControl>().SpawnEntity(prefab, new Vector3(Random.Range(point1.x, point2.x), 0, Random.Range(point1.z, point2.z)), Client);
    }

    protected override void Update()
    {
        base.Update();
        if(lastTrained + TrainingTime < Time.time && trainingQueue.Any())
        {
            CmdTrainUnit(trainingQueue.Dequeue());
            lastTrained = Time.time;
        }
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
                return building.trainingQueue.Count < TrainingQueueSize && !costs.Any(cost => building.Inventory[cost.Resource] < cost.Amount);
            }
        }

        public override void Execute()
        {
            if (building.hasAuthority)
            {
                if(building.trainingQueue.Count >= TrainingQueueSize) { return; }
                foreach (var cost in costs)
                {
                    if(!building.Inventory.RemoveResources(cost.Resource, cost.Amount)) { return; }
                }
                if (!building.trainingQueue.Any()) { building.lastTrained = Time.time; }
                building.trainingQueue.Enqueue(prefab(building));
            }
        }
    }
}
