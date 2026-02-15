using UnityEngine;

[CreateAssetMenu(fileName = "SaveSettingsSO", menuName = "ScriptableObjects/Save/SaveSettingsSO")]
public class SaveUISettingsSO : ScriptableObject {
    [Range(0, 1)] public float MasterVolume;
    [Range(0, 1)] public float MusicVolume;
    [Range(0, 1)] public float EffectsVolume;
    [Range(0, 1)] public float UIVolume;
    [Range(0, 1)] public float MouseSensitivity;

}
