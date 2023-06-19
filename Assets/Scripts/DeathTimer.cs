using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTimer : MonoBehaviour
{
    //Death timer (disable), not for use on network objects
    public float disableTimer = 5f;

    private void OnEnable()
    {
        Invoke("Disable", disableTimer);
    }

    void Disable()
    {
        gameObject.SetActive(false);
    }
}
