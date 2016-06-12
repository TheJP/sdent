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
}
