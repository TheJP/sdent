using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Worker : RtsUnit
{
    private class BuildStorageHouse : AbilityBase
    {
        public BuildStorageHouse() : base("Storage House", "Build a storage house, where workers can load off their resources.", KeyCode.Q) { }

        public override void Execute()
        {
            //TODO: Build building
        }
    }
}
