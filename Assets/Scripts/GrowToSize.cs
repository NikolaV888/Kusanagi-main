using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowToSize : MonoBehaviour
{
    public Vector3 wantedSize;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, wantedSize, Time.deltaTime * speed);
    }
}
