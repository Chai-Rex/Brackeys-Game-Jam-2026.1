using UnityEngine;

public class ToggleAudioSourceAccess : MonoBehaviour
{
    public void Toggle(AudioSource audioSource) => audioSource?.Toggle();
}
