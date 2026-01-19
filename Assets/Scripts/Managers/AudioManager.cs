using System;
using System.Diagnostics.Tracing;
using UnityEngine;

[Serializable]
public class AudioManager
{
    [SerializeField] private float masterVolume = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    public void setVolume(float volume)
    {
        masterVolume = volume;
    }
}
