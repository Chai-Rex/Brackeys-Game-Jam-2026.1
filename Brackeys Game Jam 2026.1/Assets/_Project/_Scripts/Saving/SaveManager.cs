using System.IO;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveManager", menuName = "ScriptableObjects/Managers/SaveManager")]
public class SaveManager : ScriptableObject, IInitializable {


    [SerializeField] private SaveUISettingsSO _iSaveUISettings;
#if UNITY_EDITOR
    [SerializeField] private bool _iIsDebug = false;
#endif
    public SaveUISettingsSO SaveSettingsSO { get { return _iSaveUISettings; } }

    public bool _IsInitialized => throw new System.NotImplementedException();

    public string _ManagerName => GetType().Name;

    private string SavePath => Path.Combine(Application.persistentDataPath, "settings.json");

    public async Task Initialize() {

        if (!LoadSettings()) {
            // If no save file exists, initialize defaults
            _iSaveUISettings.MasterVolume = 0.5f;
            _iSaveUISettings.MusicVolume = 0.5f;
            _iSaveUISettings.EffectsVolume = 0.5f;
            _iSaveUISettings.MouseSensitivity = 0.5f;
        }
    }

    public void CleanUp() {
        SaveSettings();
    }

    public void SaveSettings() {
        string json = JsonUtility.ToJson(_iSaveUISettings, true); // pretty print for debugging
        File.WriteAllText(SavePath, json);
#if UNITY_EDITOR
        if (_iIsDebug) Debug.Log($"Settings saved to {SavePath}");
#endif
    }


    /// <summary>
    /// Loads settings from disk. Returns true if successful.
    /// </summary>
    private bool LoadSettings() {
        if (File.Exists(SavePath)) {
            string json = File.ReadAllText(SavePath);
            JsonUtility.FromJsonOverwrite(json, _iSaveUISettings);
#if UNITY_EDITOR
            if (_iIsDebug) Debug.Log($"Settings loaded from {SavePath}");
#endif
            return true;
        }
        return false;
    }

    // TO DO:
    // Saving for Web builds? 
}
