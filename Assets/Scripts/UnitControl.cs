using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class UnitControl : MonoBehaviour
{
    private readonly IList<RtsEntity> selectedEntities = new List<RtsEntity>();

    void Update()
    {
        var leftClick = Input.GetMouseButtonDown(0);
        var rightClick = Input.GetMouseButtonDown(1);
        if (leftClick || rightClick)
        {
            //Cast a ray to determine, what was clicked
            var ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
            var hits = Physics.RaycastAll(ray);
            if (leftClick) { LeftClick(ray, hits); }
            else if (rightClick) { RightClick(hits); }
        }
        //TODO: Add proper call to this method with the gui update
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
            foreach (var selectedEntity in selectedEntities)
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
        foreach (var selectedEntity in selectedEntities)
        {
            if (selectedEntity != entity) { selectedEntity.Selected = false; }
        }
        selectedEntities.Clear();
        if (entity != null)
        {
            if (!entity.Selected) { entity.Selected = true; }
            selectedEntities.Add(entity);
        }
    }
}
