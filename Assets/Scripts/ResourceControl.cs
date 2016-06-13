using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

/// <summary>
/// Only use this class with server side scripts.
/// </summary>
public class ResourceControl : NetworkBehaviour, IEnumerable<RtsResource>
{
    public GameObject clayMinePrefab;
    [Tooltip("Tells the resource control how many clay mines it has to spawn")]
    [Range(0, 100)]
    public int clayMineAmount;

    public GameObject coalMinePrefab;
    [Tooltip("Tells the resource control how many coal mines it has to spawn")]
    [Range(0, 100)]
    public int coalMineAmount;

    public GameObject goldMinePrefab;
    [Tooltip("Tells the resource control how many gold mines it has to spawn")]
    [Range(0, 100)]
    public int goldMineAmount;

    public GameObject ironMinePrefab;
    [Tooltip("Tells the resource control how many iron mines it has to spawn")]
    [Range(0, 100)]
    public int ironMineAmount;

    public GameObject treeSourcePrefab;
    [Tooltip("Tells the resource control how many forests it has to spawn")]
    [Range(0, 100)]
    public int treeSourceAmount;

    [Tooltip("The two corner attributes define the space, where resources may be spawned")]
    public Transform spawnCorner1;
    [Tooltip("The two corner attributes define the space, where resources may be spawned")]
    public Transform spawnCorner2;

    [Tooltip("Minimal distance between resources")]
    public float minimalDistance = 10f;

    private readonly List<RtsResource> resources = new List<RtsResource>();

    /// <summary>Returns a random location to spawn a resource, which lies in the spawning boundaries and does not intersect another resource.</summary>
    private Vector3 GetSpawnPosition()
    {
        Vector3 position;
        do
        {
            position = new Vector3(Random.Range(spawnCorner1.position.x, spawnCorner2.position.x), 0, Random.Range(spawnCorner1.position.z, spawnCorner2.position.z));
        } while (resources.Any(resource => (resource.transform.position - position).sqrMagnitude < minimalDistance * minimalDistance));
        return position;
    }

    /// <summary>
    /// Distribute local resources.
    /// </summary>
    void Start()
    {
        var prefabs = new[] { clayMinePrefab, coalMinePrefab, goldMinePrefab, ironMinePrefab, treeSourcePrefab };
        var amounts = new[] { clayMineAmount, coalMineAmount, goldMineAmount, ironMineAmount, treeSourceAmount };
        for (int i = 0; i < prefabs.Length; ++i)
        {
            for (int j = 0; j < amounts[i]; ++j)
            {
                var resource = (GameObject)Instantiate(prefabs[i], GetSpawnPosition(), Quaternion.identity);
                resource.transform.parent = transform;
                NetworkServer.Spawn(resource);
                resources.Add(resource.GetComponent<RtsResource>());
            }
        }
    }

    public IEnumerator<RtsResource> GetEnumerator() { return resources.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return resources.GetEnumerator(); }
}
 