using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideableShadow : MonoBehaviour
{
    public Color desiredColor = new Color(0, 0, 0, .5f);
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!spriteRenderer) return;
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, desiredColor, Time.deltaTime * 10f);
    }
}
