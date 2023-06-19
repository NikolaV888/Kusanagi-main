using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
    Renderer renderer;
    Vector2 offset = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();   
    }

    // Update is called once per frame
    void Update()
    {
        renderer.material.SetTextureOffset("Diffuse", offset + new Vector2(Time.deltaTime, Time.deltaTime));
    }
}
