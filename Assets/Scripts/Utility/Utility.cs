using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Utility
{
    public static Vector3? RayMouseToGround()
    {
        var hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
        return hits.Where(hit => hit.transform.tag == "Ground").Select(hit => (Vector3?)hit.point).FirstOrDefault();
    }

    public static RtsEntity RayMouseToRtsEntity()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray);
        return hits
            .Where(hit => hit.transform.tag == "RtsEntity")
            .OrderBy(hit => (hit.point - ray.origin).sqrMagnitude)
            .Select(hit => hit.transform.gameObject.GetComponent<RtsEntity>())
            .FirstOrDefault();
    }

    public static IEnumerable<RaycastHit> RayMouseToGroundOrRtsEntity()
    {
        return Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition))
            .Where(hit => hit.transform.tag == "RtsEntity" || hit.transform.tag == "Ground");
    }

    public static RtsEntity GetRtsEntityFromHits(IEnumerable<RaycastHit> hits)
    {
        return hits
            .Where(hit => hit.transform.tag == "RtsEntity")
            .Select(hit => hit.transform.gameObject.GetComponent<RtsEntity>())
            .FirstOrDefault();
    }

    public static Vector3? GetGroundFromHits(IEnumerable<RaycastHit> hits)
    {
        return hits
            .Where(hit => hit.transform.tag == "Ground")
            .Select(hit => (Vector3?)hit.point)
            .FirstOrDefault();
    }

    public static RtsBuilding FindNearestStorage(EntityContainer entities, Vector3 position)
    {
        return Enumerable.Union(
            entities.Get<Saloon>().Select(saloon => saloon as RtsBuilding),
            entities.Get<StorageHouse>().Select(house => house as RtsBuilding))
        .Where(house => house.hasAuthority)
        .OrderBy(house => (house.transform.position - position).sqrMagnitude)
        .FirstOrDefault();
    }

    public static Bounds BoundsFromScreenPoints(Vector3 screenPoint1, Vector3 screenPoint2)
    {
        var position1 = Camera.main.ScreenToViewportPoint(screenPoint1);
        var position2 = Camera.main.ScreenToViewportPoint(screenPoint2);
        var boundsMin = Vector3.Min(position1, position2);
        var boundsMax = Vector3.Max(position1, position2);
        boundsMin.z = Camera.main.nearClipPlane;
        boundsMax.z = Camera.main.farClipPlane;
        var bounds = new Bounds();
        bounds.SetMinMax(boundsMin, boundsMax);
        return bounds;
    }
}
