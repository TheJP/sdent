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
}
