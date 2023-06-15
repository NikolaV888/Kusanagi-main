using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public enum weatherStates { DayClear, NightCalm, DayFoggy, NightStorm, Count }
    public int dayLength = 240;

    Animator animator;
    public StreetLightController[] streetLights;

    public bool debugWeather = false;
    public weatherStates debugWeatherState = weatherStates.DayClear;

    public void FreezeAnimation()
    {
        animator.speed = 0;
    }

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
        if (debugWeather)
        {
            animator.SetInteger("WeatherID", (int)debugWeatherState);
        }
        else
        {
            animator.SetInteger("WeatherID", (int)(NetworkTime.time / dayLength) % (int)weatherStates.Count);
        }

        UpdateStreetLights();
    }

    void UpdateStreetLights()
    {
        // Assuming NightCalm and NightStorm are the "night" states
        bool isNight = animator.GetInteger("WeatherID") == (int)weatherStates.NightCalm
                        || animator.GetInteger("WeatherID") == (int)weatherStates.NightStorm;

        foreach (StreetLightController light in streetLights)
        {
            // Check if the StreetLightController object or its Light2D is null
            if (light == null)
            {
                Debug.LogError("A StreetLightController object in the array is null");
                continue;
            }
            if (light.streetLight == null)
            {
                Debug.LogError("The Light2D object in a StreetLightController is null");
                continue;
            }

            if (isNight) light.TurnOn();
            else light.TurnOff();
        }
    }

#endif
}
