using UnityEngine;
using System.Linq;

public class UnitControl : MonoBehaviour
{
    private RtsEntity selectedEntity = null;

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
            if (leftClick)
            {
                //Find clicked unit (or none)
                var entity = hits
                    .Where(hit => hit.transform.tag == "RtsEntity")
                    .OrderBy(hit => (hit.point - ray.origin).sqrMagnitude)
                    .Select(hit => hit.transform.gameObject.GetComponent<RtsEntity>())
                    .FirstOrDefault();
                //Select clicked unit
                if (entity != selectedEntity)
                {
                    if (selectedEntity != null) { selectedEntity.Selected = false; }
                    if (entity != null) { entity.Selected = true; }
                    selectedEntity = entity;
                }
            }
            else if (rightClick)
            {

            }
        }
    }
}
