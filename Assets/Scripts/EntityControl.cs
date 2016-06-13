﻿using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;

public class EntityControl : NetworkBehaviour
{
    public GameObject constructionSitePrefab;
    public Menu menu;

    /// <summary>Determines, which entity type of the selected entities is active.</summary>
    public Type ActiveType { get; private set; }

    private readonly EntityContainer selectedEntities = new EntityContainer();
    private readonly EntityContainer entities = new EntityContainer();

    /// <summary>Returns a read only collection of all known entities. (Read Only)</summary>
    public EntityContainer Entities { get { return entities.AsReadOnly(); } }
    /// <summary>Returns a read only collection of all selected entities. This should only be used on the client. (Read Only)</summary>
    public EntityContainer SelectedEntities { get { return selectedEntities.AsReadOnly(); } }


    void Update()
    {
        var leftClick = Input.GetMouseButtonDown(0);
        var rightClick = Input.GetMouseButtonDown(1);
        if(leftClick || rightClick)
        {
            var clickedEntity = Utility.RayMouseToRtsEntity();
            if (leftClick)
            {
                if (!menu.HandleMouseClick(Input.mousePosition)) { LeftClick(clickedEntity); }
            }
            else if (rightClick)
            {
                RightClick(clickedEntity);
            }
        }
        //Execute abilities of active entities if possible
        foreach(var ability in selectedEntities.Get(ActiveType).ToList().SelectMany(entity => entity.Abilities))
        {
            if (ability.CanExecute && Input.GetKeyDown(ability.Key))
            {
                ability.Execute();
            }
        }
    }

    private void RightClick(RtsEntity clickedEntity)
    {
        //Execute right click ability on all selected units
        foreach (var selectedEntity in selectedEntities.ToList())
        {
            var targetAbility = selectedEntity.RightClickWithTargetAbility;
            if (targetAbility != null && clickedEntity != null && targetAbility.CanExecute) { targetAbility.Execute(clickedEntity); }
            else
            {
                var ability = selectedEntity.RightClickAbility;
                if (ability != null && ability.CanExecute) { ability.Execute(); }
            }
        }
    }

    private void LeftClick(RtsEntity clickedEntity)
    {
        //Select clicked unit
        foreach (var selectedEntity in selectedEntities.ToList())
        {
            if (selectedEntity != clickedEntity) { selectedEntity.Selected = false; }
        }
        selectedEntities.Clear();
        if (clickedEntity != null)
        {
            if (!clickedEntity.Selected) { clickedEntity.Selected = true; }
            selectedEntities.Add(clickedEntity);
            ActiveType = clickedEntity.GetType();
        }
    }

    private void EntityDied(RtsEntity entity)
    {
        selectedEntities.Remove(entity);
        entities.Remove(entity);
        if (ActiveType != null && !selectedEntities.ContainsType(ActiveType))
        {
            ActiveType = selectedEntities.Select(activeEntity => activeEntity.GetType()).FirstOrDefault();
        }
    }

    [Server]
    private GameObject Spawn(GameObject prefab, Vector3 position, NetworkConnection player, System.Action<RtsEntity> initalise = null)
    {
        var entity = (GameObject)Instantiate(prefab, position, prefab.transform.rotation);
        entity.transform.parent = transform;
        var rtsEntity = entity.GetComponent<RtsEntity>();
        rtsEntity.SetClient(player);
        if (initalise != null) { initalise(rtsEntity); }
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
    public void BuildConstructionSite(Buildings finalBuilding, Vector3 position, NetworkConnection player, GameObject worker)
    {
        var constructionSite = Spawn(constructionSitePrefab, position, player, rtsEntity => (rtsEntity as ConstructionSite).FinalBuilding = finalBuilding);
        var site = constructionSite.GetComponent<ConstructionSite>();
        worker.GetComponent<Worker>().RpcAssignWork(constructionSite);
    }
}
