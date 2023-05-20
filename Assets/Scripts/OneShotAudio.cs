using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotAudio : MonoBehaviour
{
    public AudioClip clip;
    public float volume = 1f;
    public AudioSource source;

    private void OnEnable()
    {
        source.PlayOneShot(clip, volume);
    }
}
