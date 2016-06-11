using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ConstructionSite : RtsBuilding
{
    private GameObject finalBuildingPrefab;

    [ClientRpc]
    public void RpcSetFinalBuilding(GameObject finalBuildingPrefab)
    {
        //Everyone is allowed to know, which building this construction site will be when done
        this.finalBuildingPrefab = finalBuildingPrefab;
    }
}
