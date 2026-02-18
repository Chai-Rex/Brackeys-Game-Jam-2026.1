using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraManager", menuName = "ScriptableObjects/Managers/CameraManager")]
public class CameraManager : ScriptableObject, IUpdateable {

    [Header("Debug Settings")]
    [SerializeField] private bool _iEnableDebugLogs = false;

    private CameraHandler _cameraHandler;
    private bool _isInitialized = false;

    public string _ManagerName => GetType().Name;

    /// <summary>
    /// Register the player handler instance (called by CameraHandler.Awake)
    /// </summary>
    public async void SetPlayerHandler(CameraHandler handler) {
        if (_cameraHandler != null && _cameraHandler != handler) {
            LogWarning($"CameraHandler already registered! Replacing...");
        }

        _cameraHandler = handler;
        Log($"CameraHandler registered");

        await Awaitable.NextFrameAsync();

        // Initialize the player handler now that it's registered
        _cameraHandler.Initialize();
        _isInitialized = true;
        Log($"Initialized successfully");
    }

    public void OnUpdate() {
        if (!_isInitialized || _cameraHandler == null) return;
        _cameraHandler.OnUpdate();
    }

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    private void Log(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.Log($"[PlayerManager] {i_message}");
        }
    }

    private void LogWarning(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.LogWarning($"[PlayerManager] {i_message}");
        }
    }

    private void LogError(string i_message) {
        Debug.LogError($"[PlayerManager] {i_message}");
    }

    #endregion
}
