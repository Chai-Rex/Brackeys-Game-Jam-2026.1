using Unity.VisualScripting;
using UnityEngine;

public static class AudioExtension
{
    public static void Play(AudioClip clip, float volume = 1, float pitch = 1, bool timeScaled = true)
    {
        volume = Mathf.Clamp(volume, 0, 1);

        if (!clip || 
            pitch == 0 || 
            volume == 0)
            return;
        
        AudioSource audioSource = new GameObject("PlayClip").AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.spatialBlend = 0;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        
        if (timeScaled)
            audioSource.AddComponent<TimeScaleAudioSource>().AudioSource = audioSource;

        if (pitch < 0) 
            audioSource.time = Mathf.Max(0, audioSource.clip.length - 0.001f);
            
        audioSource.Play();

        Object.Destroy(audioSource.gameObject, clip.length / Mathf.Abs(pitch) + 0.1f);
    }


    public static void Toggle(this AudioSource source)
    {
        if (source.isPlaying) source.Stop();
        else                  source.Play();
    }
}


