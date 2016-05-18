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
    public GameObject goldMinePrefab;
    [Tooltip("Tells the resource control how many gold mines it has to spawn")]
    [Range(0, 100)]
    public int goldMineAmount;

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
        for (int i = 0; i < goldMineAmount; ++i)
        {
            var mine = (GameObject)Instantiate(goldMinePrefab, GetSpawnPosition(), Quaternion.identity);
            mine.transform.parent = transform;
            NetworkServer.Spawn(mine);
            resources.Add(mine.GetComponent<RtsResource>());
        }
    }

    public IEnumerator<RtsResource> GetEnumerator() { return resources.GetEnumerator(); }
    IEnumerator IEnumerable.GetEnumerator() { return resources.GetEnumerator(); }
}
 