using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PixelPerfectCameraModifier : MonoBehaviour
{
    PixelPerfectCamera pixelPerfectCamera;
    float referenceResolution;
    // Start is called before the first frame update
    void Start()
    {
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        referenceResolution = pixelPerfectCamera.refResolutionX;
    }

    // Update is called once per frame
    void Update()
    {
        if (UIUtils.AnyInputActive()) return;

        if (Input.GetKey(KeyCode.Equals) && referenceResolution < 420) // Equals key corresponds to "+/=" on the keyboard
        {
            referenceResolution += Time.deltaTime * (referenceResolution) * 1.2f; // Increased zoom-out factor to 1.2 for more zoom out
            pixelPerfectCamera.refResolutionX = (int)referenceResolution;
            pixelPerfectCamera.refResolutionY = (int)referenceResolution;
        }
        else if (Input.GetKey(KeyCode.Minus) && pixelPerfectCamera.refResolutionX > 64) // Minus key corresponds to "_/-" on the keyboard
        {
            referenceResolution -= Time.deltaTime * (referenceResolution) * 1.1f;
            pixelPerfectCamera.refResolutionX = (int)referenceResolution;
            pixelPerfectCamera.refResolutionY = (int)referenceResolution;
        }
    }
}
