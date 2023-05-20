using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public enum weatherStates { DayClear, NightCalm, DayFoggy, NightStorm, Count }
    public int dayLength = 240;

    Animator animator;

    public bool debugWeather = false;
    public weatherStates debugWeatherState = weatherStates.DayClear;

    private void Awake()
    {
#if !UNITY_SERVER || UNITY_EDITOR
        animator = GetComponent<Animator>();
        animator.speed = 1000f;
        Invoke("OnSecond", 1f);
#else
        gameObject.SetActive(false);
#endif
    }

#if !UNITY_SERVER || UNITY_EDITOR
    void OnSecond()
    {
        animator.speed = 1f;
    }

    private void Update()
    {
        if(debugWeather) animator.SetInteger("WeatherID", (int)debugWeatherState);
        else animator.SetInteger("WeatherID", (int)(NetworkTime.time / dayLength) % (int)weatherStates.Count);
    }
#endif
}
