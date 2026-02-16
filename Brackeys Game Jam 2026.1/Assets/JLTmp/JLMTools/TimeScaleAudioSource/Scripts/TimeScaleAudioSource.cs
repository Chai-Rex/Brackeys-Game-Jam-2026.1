using System.Collections.Generic;
using UnityEngine;

// Use TimeExtension.TimeScale instead of Time.timeScale to track timeScale changes. 
// Use AudioSource.SetPitch instead of AudioSource.pitch to check if the AudioSource is time scaled.
public class TimeScaleAudioSource : MonoBehaviour
{
    static HashSet<AudioSource> linkedAudioSources = new();

    [SerializeField] AudioSource audioSource;
    AudioSource _audioSource;
    bool awakened = false;
    bool linked = false;


    public AudioSource AudioSource
    {
        get => audioSource;

        set {
            if (audioSource == value)
                return;
            
            if (linked)
            {
                if (enabled) 
                    audioSource.SetTimeScaled(false);

                linkedAudioSources.Remove(audioSource);
                linked = false;
            }

            audioSource = value;

            if (awakened)
            {
                if (linkedAudioSources.Contains(audioSource))
                {
                    Debug.LogWarning("TimeScaleAudioSource.AudioSource : AudioSource already linked. It has been set to null.");
                    audioSource = null;
                }

                else if (audioSource)
                {
                    linkedAudioSources.Add(audioSource);
                    linked = true;

                    if (enabled)
                        audioSource.SetTimeScaled(true);
                }

                _audioSource = audioSource;
            }
        }
    }

    void Reset()
    {
        if (GetComponents<AudioSource>().Length == 1 &&
            GetComponents<TimeScaleAudioSource>().Length == 1)
            audioSource = GetComponent<AudioSource>();
    }

    void OnValidate()
    {
        if (awakened &&
            _audioSource != audioSource)
        {
            AudioSource newAudioSource = audioSource;
            audioSource = _audioSource;
            AudioSource = newAudioSource;
        }
    }

    void Awake()
    {
        if (linkedAudioSources.Contains(audioSource))
        {
            Debug.LogWarning("TimeScaleAudioSource.Awake : AudioSource already linked. It has been set to null.");
            audioSource = null;
        }

        else if (audioSource)
        {
            linkedAudioSources.Add(audioSource);
            linked = true;
        }

        _audioSource = audioSource;
        awakened = true;
    }

    void OnDestroy()
    {
        if (linked)
        {
            linkedAudioSources.Remove(audioSource);
            linked = false;
        }
    }

    void OnEnable()
    {
        if (linked) 
            audioSource.SetTimeScaled(true);
    }

    void OnDisable()
    {
        if (linked)
            audioSource.SetTimeScaled(false);
    }
}