using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0, 1)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool rangePitch = false;
    [Range(0.1f, 3f)]
    public float minPitch = 1f;
    [Range(0.1f, 3f)]
    public float maxPitch = 1f;
    public AudioMixerGroup mixer;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

}
