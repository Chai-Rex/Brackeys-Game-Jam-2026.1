using UnityEngine;

public class PlayAudioClipAccess : MonoBehaviour
{
    public void Play(AudioClip clip) => AudioExtension.Play(clip);
}
