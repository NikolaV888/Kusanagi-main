using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideShadows : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<HideableShadow>() && collision.GetComponent<SpriteRenderer>()) collision.GetComponent<HideableShadow>().desiredColor = Color.clear;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<HideableShadow>() && collision.GetComponent<SpriteRenderer>()) collision.GetComponent<HideableShadow>().desiredColor = new Color(0,0,0,.5f);
    }
}
