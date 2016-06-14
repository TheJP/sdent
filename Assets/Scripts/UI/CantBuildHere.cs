using UnityEngine;
using System.Collections;

public class CantBuildHere : MonoBehaviour
{
    public const float StayingTime = 2f;
    public const float FadingTime = 5f;

    public MeshRenderer meshRenderer;

    private float startTime;
    private float startAlpha;

    void Start()
    {
        startTime = Time.time;
        startAlpha = meshRenderer.material.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        if(startTime + StayingTime < Time.time)
        {
            if (startTime + StayingTime + FadingTime < Time.time) { Destroy(gameObject); }
            else
            {
                var material = meshRenderer.material;
                var newAlpha = startAlpha * ((Time.time - startTime) / (startTime + StayingTime + FadingTime));
                Debug.Log((startTime - Time.time) / (startTime + StayingTime + FadingTime));
                material.SetColor("_Color", new Color(material.color.r, material.color.g, material.color.b, newAlpha));
            }
        }
    }
}
