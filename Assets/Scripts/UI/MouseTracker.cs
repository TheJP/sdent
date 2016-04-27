using UnityEngine;
using System.Collections;

public class MouseTracker : MonoBehaviour
{
    private bool mouseInWindow = false;
    public bool MouseInWindow { get { return mouseInWindow; } }

    void OnMouseEnter()
    {
        mouseInWindow = true;
    }

    void OnMouseExit()
    {
        mouseInWindow = false;
    }
}
