using UnityEngine;
using System.Collections;
using System.Linq;

public class CameraControl : MonoBehaviour
{
    public float cameraSpeedKeyboard;
    public float cameraSpeedMouse;
    public float rotation;
    public MouseTracker mouseTracker;

    private bool hasInitialPosition = false;

    /// <summary>
    /// Defines how wide / high the border is in pixels
    /// When the mouse is in this border, the camera is moved.
    /// </summary>
    public Vector2 borderSize;

    /// <summary>
    /// Set the camera position at the start of the game to focus the initial unit.
    /// </summary>
    public void SetInitialPosition(RtsUnit unit)
    {
        if (hasInitialPosition) { return; }
        if (unit != null)
        {
            hasInitialPosition = true;
            var toCamera = transform.rotation * Vector3.back;
            var factor = (transform.position.y - unit.transform.position.y) / toCamera.y;
            transform.position = unit.transform.position + factor * toCamera;
        }
    }

    void Update()
    {
        //Camera movement (keyboard)
        var direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        var hasKeyboardInput = direction.sqrMagnitude > 0;
        //Camera movement (mouse)
        if (!hasKeyboardInput && mouseTracker.MouseInWindow)
        {
            direction = new Vector3(
                Input.mousePosition.x <= borderSize.x ? -1 : (Input.mousePosition.x >= Screen.width - borderSize.x ? 1 : 0), 0,
                Input.mousePosition.y <= borderSize.y ? -1 : (Input.mousePosition.y >= Screen.height - borderSize.y ? 1 : 0)
            );
        }
        if (direction.sqrMagnitude > 0)
        {
            var speed = Time.deltaTime * (hasKeyboardInput ? cameraSpeedKeyboard : cameraSpeedMouse);
            transform.position += speed * (Quaternion.AngleAxis(rotation, Vector3.up) * direction.normalized);
        }
    }
}
