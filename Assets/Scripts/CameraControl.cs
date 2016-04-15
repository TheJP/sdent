using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    public float cameraSpeed;
    public float rotation;

    void Update()
    {
        //Camera movement
        var direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        transform.position += Time.deltaTime * cameraSpeed * (Quaternion.AngleAxis(rotation, Vector3.up) * direction);
    }
}
