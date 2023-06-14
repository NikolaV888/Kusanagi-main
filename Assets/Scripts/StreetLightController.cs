using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StreetLightController : MonoBehaviour
{
    public Light2D streetLight;
    public float maxIntensity = 0.5f;
    public float minIntensity = 0f;

    public void TurnOn()
    {
        StartCoroutine(FadeLight(streetLight, maxIntensity));
    }

    public void TurnOff()
    {
        StartCoroutine(FadeLight(streetLight, minIntensity));
    }

    IEnumerator FadeLight(Light2D lightToFade, float targetIntensity)
    {
        float duration = 1f; // Duration for fade effect. You can adjust this.
        float startIntensity = lightToFade.intensity;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            // Update intensity
            lightToFade.intensity = Mathf.Lerp(startIntensity, targetIntensity, t / duration);
            //Debug.Log("Fading light to " + lightToFade.intensity); // log the current intensity
            yield return null;
        }

        lightToFade.intensity = targetIntensity; // Ensure target intensity is set exactly
      //  Debug.Log("Light intensity set to " + lightToFade.intensity); // log the final intensity
    }
}
