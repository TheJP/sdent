using UnityEngine;
using System.Collections;

public class UIDirectionControl : MonoBehaviour
{

    public bool useStaticRotation = true;
    private Quaternion rotation;

    void Start()
    {
        rotation = transform.parent.localRotation;
    }

    void Update()
    {
        if (useStaticRotation) { transform.rotation = rotation; }
    }
}
