using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;

public class EntityControl : NetworkBehaviour
{
    public GameObject constructionSitePrefab;

    /// <summary>Determines, which entity type of the selected entities is active.</summary>
    private System.Type activeType;

    private readonly EntityContainer selectedEntities = new EntityContainer();
    private readonly EntityContainer entities = new EntityContainer();

    public IEnumerable<RtsEntity> Entities { get { return entities.ToList(); } }
    public IEnumerable<RtsEntity> SelectedEntities { get { return selectedEntities.ToList(); } }

    void Update()
    {
        var leftClick = Input.GetMouseButtonDown(0);
        var rightClick = Input.GetMouseButtonDown(1);
        if (leftClick)
        {
            //Cast a ray to determine, what was clicked
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
            var hits = Physics.RaycastAll(ray);
            LeftClick(ray, hits);
        }
        else if(rightClick)
        {
            RightClick();
        }
        //Execute abilities of active entities if possible
        foreach(var ability in selectedEntities.Get(activeType).ToList().SelectMany(entity => entity.Abilities))
        {
            if (ability.CanExecute && Input.GetKeyDown(ability.Key))
            {
                ability.Execute();
            }
        }
    }

    private void RightClick()
    {
        //Execute right click ability on all selected units
        foreach (var selectedEntity in selectedEntities.ToList())
        {
            var ability = selectedEntity.RightClickAbility;
            if (ability != null && ability.CanExecute) { ability.Execute(); }
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
        foreach (var selectedEntity in selectedEntities.ToList())
        {
            if (selectedEntity != entity) { selectedEntity.Selected = false; }
        }
        selectedEntities.Clear();
        if (entity != null)
        {
            if (!entity.Selected) { entity.Selected = true; }
            selectedEntities.Add(entity);
            activeType = entity.GetType();
        }
    }

    private void EntityDied(RtsEntity entity)
    {
        selectedEntities.Remove(entity);
        entities.Remove(entity);
        if (activeType != null && !selectedEntities.ContainsType(activeType))
        {
            activeType = selectedEntities.Select(activeEntity => activeEntity.GetType()).FirstOrDefault();
        }
    }

    [Server]
    private GameObject Spawn(GameObject prefab, Vector3 position, NetworkConnection player)
    {
        var entity = (GameObject)Instantiate(prefab, position, prefab.transform.rotation);
        entity.transform.parent = transform;
        var rtsEntity = entity.GetComponent<RtsEntity>();
        rtsEntity.SetClient(player);
        NetworkServer.SpawnWithClientAuthority(entity, player);
        return entity;
    }

    /// <summary>Add entity to the entity control. Can be called on the server and on the client.</summary>
    /// <param name="entity"></param>
    public void AddEntity(RtsEntity entity)
    {
        entities.Add(entity);
        entity.EntityDied += EntityDied;
    }

    /// <summary>Server method, which spawns units and buildings with client authority. Should never be used with resources (e.g. GoldMine).</summary>
    /// <param name="entityPrefab"></param>
    /// <param name="position"></param>
    /// <param name="player"></param>
    [Server]
    public void SpawnEntity(GameObject entityPrefab, Vector3 position, NetworkConnection player)
    {
        Spawn(entityPrefab, position, player);
    }

    /// <summary>Server method, which spawns a construction site and assigns the given worker to it.</summary>
    /// <param name="finalBuildingPrefab">Prefab of the building, which will spawn, after the construction site is finished.</param>
    /// <param name="position"></param>
    /// <param name="player"></param>
    /// <param name="worker">Worker, which will get the job to work on the construction site.</param>
    [Server]
    public void BuildConstructionSite(GameObject finalBuildingPrefab, Vector3 position, NetworkConnection player, GameObject worker)
    {
        var constructionSite = Spawn(constructionSitePrefab, position, player);
        var site = constructionSite.GetComponent<ConstructionSite>();
        worker.GetComponent<Worker>().RpcAssignWork(constructionSite);Debug.Log(finalBuildingPrefab);
        site.RpcSetFinalBuilding(finalBuildingPrefab);
    }
}
