using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite destroyedSprite;

    public void Destroy()
    {
        if (GetComponent<SpriteRenderer>().sprite == destroyedSprite) return;
        GetComponent<SpriteRenderer>().sprite = destroyedSprite;
    }
}
