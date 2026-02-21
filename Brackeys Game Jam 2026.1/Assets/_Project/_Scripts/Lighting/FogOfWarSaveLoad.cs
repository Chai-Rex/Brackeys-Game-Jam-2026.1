using System.IO;
using UnityEngine;

/// <summary>
/// Saves and loads the FogOfWarManager discovery map to and from disk.
///
/// USAGE:
///   Call SaveDiscoveryMap() when saving the game.
///   Call LoadDiscoveryMap() after the scene and tilemap are fully loaded.
///   Use a different saveSlot string per save file.
/// </summary>
public class FogOfWarSaveLoad : MonoBehaviour {
    public FogOfWarManager fogManager;

    [Tooltip("Filename for the fog save data. Change per save slot.")]
    public string saveSlot = "slot0_fog.dat";

    void Awake() {
        if (fogManager == null)
            fogManager = FindFirstObjectByType<FogOfWarManager>();
    }

    public void SaveDiscoveryMap() {
        if (fogManager == null) return;
        try {
            byte[] data = fogManager.SerializeDiscoveryMap();
            File.WriteAllBytes(GetPath(), data);
            Debug.Log("[FogOfWar] Saved to " + GetPath() + " (" + data.Length + " bytes)");
        } catch (System.Exception e) {
            Debug.LogError("[FogOfWar] Save failed: " + e.Message);
        }
    }

    public void LoadDiscoveryMap() {
        if (fogManager == null) return;
        string path = GetPath();
        if (!File.Exists(path)) {
            Debug.Log("[FogOfWar] No save file found at " + path + ". Starting fresh.");
            return;
        }
        try {
            fogManager.DeserializeDiscoveryMap(File.ReadAllBytes(path));
            Debug.Log("[FogOfWar] Loaded from " + path);
        } catch (System.Exception e) {
            Debug.LogError("[FogOfWar] Load failed: " + e.Message);
        }
    }

    public void DeleteSave() {
        string path = GetPath();
        if (File.Exists(path))
            File.Delete(path);
    }

    string GetPath() {
        return Path.Combine(Application.persistentDataPath, saveSlot);
    }
}