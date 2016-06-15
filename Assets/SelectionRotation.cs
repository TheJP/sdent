using UnityEngine;
using System.Collections;

public class SelectionRotation : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * 120);
    }
}
