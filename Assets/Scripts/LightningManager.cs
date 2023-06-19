using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningManager : MonoBehaviour
{
    public static LightningManager Instance;
    public float shortestLightingTimer = 1f;
    public float longestLightingTimer = 5f;

    public Animator[] animators;

    float lightningTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    [ClientCallback]
    private void Update()
    {
        if (lightningTimer < 0f)
        {
            lightningTimer = Random.Range(shortestLightingTimer, longestLightingTimer);
            ShowLightning();
        }
        else lightningTimer -= Time.deltaTime;
    }

    void ShowLightning()
    {
        animators[Random.Range(0, animators.Length)].gameObject.SetActive(true);
    }
}
