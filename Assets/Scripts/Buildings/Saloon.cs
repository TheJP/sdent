using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Saloon : RtsTrainingBuilding
{
    public Transform spawnPoint;

    private readonly List<IAbility> abilities = new List<IAbility>();
    public override IEnumerable<IAbility> Abilities
    {
        get { return abilities; }
    }

    public Saloon()
    {
        abilities.Add(new TrainWorker(this));
    }

    private class TrainWorker : AbilityBase
    {
        private Saloon saloon;
        public TrainWorker(Saloon saloon) : base("Worker", "")
        {
            this.saloon = saloon;
        }

        public override void Execute()
        {
        }
    }
}
