using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PixelPerfectCameraModifier : MonoBehaviour
{
    PixelPerfectCamera pixelPerfectCamera;
    float referenceResolution;
    float originalResolution;
    bool isZoomedOut = false;

    // Start is called before the first frame update
    void Start()
    {
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        originalResolution = pixelPerfectCamera.refResolutionX;
        referenceResolution = originalResolution;
    }

    // Update is called once per frame
    void Update()
    {
        if (UIUtils.AnyInputActive()) return;

        if (Input.GetKey(KeyCode.Equals) && referenceResolution < 420)
        {
            isZoomedOut = true;
            referenceResolution += Time.deltaTime * (referenceResolution) * 1.2f;
            pixelPerfectCamera.refResolutionX = (int)referenceResolution;
            pixelPerfectCamera.refResolutionY = (int)referenceResolution;
        }
        else if (Input.GetKey(KeyCode.Minus) && isZoomedOut)
        {
            referenceResolution = originalResolution;
            pixelPerfectCamera.refResolutionX = (int)referenceResolution;
            pixelPerfectCamera.refResolutionY = (int)referenceResolution;
            isZoomedOut = false;
        }
        else if (Input.GetKey(KeyCode.Equals) && isZoomedOut)
        {
            isZoomedOut = true;
            referenceResolution += Time.deltaTime * (referenceResolution) * 1.3f;
            pixelPerfectCamera.refResolutionX = (int)referenceResolution;
            pixelPerfectCamera.refResolutionY = (int)referenceResolution;
        }
    }
}
