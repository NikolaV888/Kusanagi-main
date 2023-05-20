using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickeringLight : MonoBehaviour
{
    Light2D light;
    float intensity;

    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light2D>();
        intensity = light.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        light.intensity = Random.Range(intensity * 0.9f, intensity * 1.1f);
    }
}
