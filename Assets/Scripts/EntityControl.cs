using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;

public class EntityControl : NetworkBehaviour
{
    public Camera rtsCamera;

    private readonly List<RtsEntity> selectedEntities = new List<RtsEntity>();
    /// <summary>To avoid enumerator exceptions, try to iterate over the cloned list when possible.</summary>
    public IEnumerable<RtsEntity> ClonedSelectedEntities
    {
        get { return selectedEntities.ToList(); }
    }

    void Update()
    {
        var leftClick = Input.GetMouseButtonDown(0);
        var rightClick = Input.GetMouseButtonDown(1);
        if (leftClick || rightClick)
        {
            //Cast a ray to determine, what was clicked
            var ray = rtsCamera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
            var hits = Physics.RaycastAll(ray);
            if (leftClick) { LeftClick(ray, hits); }
            else if (rightClick) { RightClick(hits); }
        }
        //TODO: Only execute abilities on active entities (subset of selected entities)
        //TODO: Check per ability for correct keys
        if (Input.GetKeyDown("b"))
        {
            foreach(var ability in ClonedSelectedEntities.SelectMany(entity => entity.Abilities))
            {
                if (ability.CanExecute) { ability.Execute(); }
            }
        }
    }

    private void RightClick(RaycastHit[] hits)
    {
        //Find point on the ground, which was selected
        var target = hits
            .Where(hit => hit.transform.tag == "Ground")
            .Select(hit => (Vector3?)hit.point)
            .FirstOrDefault();
        //Execute right click action on selected units
        if (target.HasValue)
        {
            foreach (var selectedEntity in ClonedSelectedEntities)
            {
                selectedEntity.DoRightClickAction(target.Value);
            }
        }
    }

    private void LeftClick(Ray ray, RaycastHit[] hits)
    {
        //Find clicked unit (or none)
        var entity = hits
            .Where(hit => hit.transform.tag == "RtsEntity")
            .OrderBy(hit => (hit.point - ray.origin).sqrMagnitude)
            .Select(hit => hit.transform.gameObject.GetComponent<RtsEntity>())
            .FirstOrDefault();
        //Select clicked unit
        foreach (var selectedEntity in ClonedSelectedEntities)
        {
            if (selectedEntity != entity)
            {
                selectedEntity.Selected = false;
                selectedEntity.EntityDied -= EntityDied;
            }
        }
        selectedEntities.Clear();
        if (entity != null)
        {
            if (!entity.Selected)
            {
                entity.Selected = true;
                entity.EntityDied += EntityDied;
            }
            selectedEntities.Add(entity);
        }
    }

    private void EntityDied(RtsEntity entity) { selectedEntities.Remove(entity); }

    /// <summary>Server method, which spawns units and buildings with client authority. Should never be used with resources (e.g. GoldMine).</summary>
    /// <param name="entityPrefab"></param>
    /// <param name="position"></param>
    /// <param name="player"></param>
    [Server]
    public void SpawnEntity(GameObject entityPrefab, Vector3 position, NetworkConnection player)
    {
        var entity = (GameObject)Instantiate(entityPrefab, position, entityPrefab.transform.rotation);
        entity.transform.parent = transform;
        entity.GetComponent<RtsEntity>().SetClient(player);
        NetworkServer.SpawnWithClientAuthority(entity, player);
    }
}
