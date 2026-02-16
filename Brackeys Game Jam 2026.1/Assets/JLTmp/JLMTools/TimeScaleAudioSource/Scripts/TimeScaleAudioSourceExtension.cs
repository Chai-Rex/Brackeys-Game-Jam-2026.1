using System.Collections.Generic;
using UnityEngine;

public static class TimeScaleAudioSourceExtension
{
    static Dictionary<AudioSource, float> dico = new();

    public static bool IsTimeScaled(this AudioSource audioSource) => dico.ContainsKey(audioSource);

    public static void SetPitch(this AudioSource audioSource, float pitch)
    {
        if (dico.ContainsKey(audioSource))
        {
            dico[audioSource] = pitch;
            audioSource.pitch = pitch * Time.timeScale;
        }
        else audioSource.pitch = pitch;
    }

    public static void SetTimeScaled(this AudioSource audioSource, bool timeScaled)
    {
        if (timeScaled) Add(audioSource);
        else            Remove(audioSource);
    }

    static bool Add(AudioSource audioSource)
    {
        if (!audioSource ||
            dico.ContainsKey(audioSource))
            return false;

        dico.Add(audioSource, audioSource.pitch);
        audioSource.pitch *= Time.timeScale;

        if (dico.Count == 1)
            TimeExtension.OnTimeScaleChanged.Add(OnTimeScaleChanged);

        //Debug.Log("Added");
        return true;
    }

    static bool Remove(AudioSource audioSource)
    {
        if (ReferenceEquals(audioSource, null) ||
            !dico.TryGetValue(audioSource, out float unscaledPitch))
            return false;

        dico.Remove(audioSource);

        if (audioSource)
            audioSource.pitch = unscaledPitch;

        if (dico.Count == 0)
            TimeExtension.OnTimeScaleChanged.Remove(OnTimeScaleChanged);

        //Debug.Log("Removed");
        return true;
    }

    static void OnTimeScaleChanged(float timeScale)
    {
        HashSet<AudioSource> toRemove = new();
        
        foreach (KeyValuePair<AudioSource, float> kv in dico)
        {
            if (kv.Key) kv.Key.pitch = kv.Value * timeScale;            
            else toRemove.Add(kv.Key);
        }

        if (toRemove.Count > 0)
        {
            Debug.LogWarning("TimeScaleAudioSourceExtension.OnTimeScaleChanged : null or destroyed AudioSources has been removed.");
            
            foreach (AudioSource audioSource in toRemove)
                dico.Remove(audioSource);

            if (dico.Count == 0)
                TimeExtension.OnTimeScaleChanged.Remove(OnTimeScaleChanged);
        }
    }
}
