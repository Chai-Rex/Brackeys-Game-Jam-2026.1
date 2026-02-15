using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages camera activation and switching between bootstrap camera and scene cameras.
/// Prevents "Multiple Audio Listeners" errors by ensuring only one camera is active at a time.
/// Handles smooth transitions between loading screens and gameplay.
/// </summary>
[CreateAssetMenu(fileName = "CameraManager", menuName = "ScriptableObjects/Managers/CameraManager")]
public class CameraManager : ScriptableObject, IInitializable, IPersistentManager {

    [Header("Debug Settings")]
    [SerializeField] private bool _iEnableDebugLogs = false;

    // State
    private bool _isInitialized = false;
    private Camera _bootstrapCamera;
    private Camera _activeSceneCamera;
    private List<Camera> _registeredSceneCameras = new List<Camera>();

    // Audio Listeners
    private AudioListener _bootstrapAudioListener;
    private AudioListener _activeAudioListener;

    // Properties
    public bool _IsInitialized => _isInitialized;
    public string _ManagerName => GetType().Name;
    public Camera _ActiveCamera => _activeSceneCamera != null ? _activeSceneCamera : _bootstrapCamera;
    public bool _IsUsingBootstrapCamera => _activeSceneCamera == null;

    ////////////////////////////////////////////////////////////
    #region Initialization
    ////////////////////////////////////////////////////////////

    public Task Initialize() {
        if (_isInitialized) {
            LogWarning("Already initialized");
            return Task.CompletedTask;
        }

        Log("Initializing...");
        _registeredSceneCameras.Clear();
        _isInitialized = true;
        return Task.CompletedTask;
    }

    public void CleanUp() {
        if (!_isInitialized) return;

        Log("Cleaning up...");

        // Unregister all scene cameras
        _registeredSceneCameras.Clear();
        _activeSceneCamera = null;

        // Note: Don't destroy cameras here - they're managed by GameBootstrap
        _bootstrapCamera = null;
        _bootstrapAudioListener = null;
        _activeAudioListener = null;

        _isInitialized = false;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Bootstrap Camera Management
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Register the persistent bootstrap camera.
    /// Called by GameBootstrap during initialization.
    /// </summary>
    public void RegisterBootstrapCamera(Camera i_camera) {
        if (i_camera == null) {
            LogError("Cannot register null bootstrap camera");
            return;
        }

        _bootstrapCamera = i_camera;
        _bootstrapAudioListener = i_camera.GetComponent<AudioListener>();

        // Initially active (for loading screens)
        ActivateCamera(_bootstrapCamera, _bootstrapAudioListener);

        Log($"Bootstrap camera registered: {i_camera.name}");
    }

    /// <summary>
    /// Activate the bootstrap camera (for loading screens).
    /// </summary>
    public void ShowBootstrapCamera() {
        if (_bootstrapCamera == null) {
            LogWarning("Bootstrap camera not registered");
            return;
        }

        Log("Switching to bootstrap camera (loading screen)");

        // Deactivate scene camera
        if (_activeSceneCamera != null) {
            DeactivateCamera(_activeSceneCamera);
        }

        // Activate bootstrap camera
        ActivateCamera(_bootstrapCamera, _bootstrapAudioListener);
        _activeSceneCamera = null;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Scene Camera Management
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Register a scene camera. Automatically switches to it.
    /// Called by SceneCamera component on Awake.
    /// </summary>
    public void RegisterSceneCamera(Camera i_camera) {
        if (i_camera == null) {
            LogError("Cannot register null scene camera");
            return;
        }

        // Check if already registered
        if (_registeredSceneCameras.Contains(i_camera)) {
            LogWarning($"Scene camera already registered: {i_camera.name}");
            return;
        }

        _registeredSceneCameras.Add(i_camera);
        Log($"Scene camera registered: {i_camera.name}");

        // Switch to this camera immediately
        SwitchToSceneCamera(i_camera);
    }

    /// <summary>
    /// Unregister a scene camera when it's destroyed.
    /// Called by SceneCamera component on OnDestroy.
    /// </summary>
    public void UnregisterSceneCamera(Camera i_camera) {
        if (i_camera == null) return;

        if (_registeredSceneCameras.Remove(i_camera)) {
            Log($"Scene camera unregistered: {i_camera.name}");

            // If this was the active camera, switch back to bootstrap
            if (_activeSceneCamera == i_camera) {
                _activeSceneCamera = null;
                ShowBootstrapCamera();
            }
        }
    }

    /// <summary>
    /// Switch to a specific scene camera.
    /// </summary>
    public void SwitchToSceneCamera(Camera i_camera) {
        if (i_camera == null) return;

        Log($"Switching to scene camera: {i_camera.name}");

        // Deactivate bootstrap camera
        if (_bootstrapCamera != null) {
            DeactivateCamera(_bootstrapCamera);
        }

        // Deactivate previous scene camera
        if (_activeSceneCamera != null && _activeSceneCamera != i_camera) {
            DeactivateCamera(_activeSceneCamera);
        }

        // Activate new scene camera
        AudioListener sceneAudioListener = i_camera.GetComponent<AudioListener>();
        ActivateCamera(i_camera, sceneAudioListener);
        _activeSceneCamera = i_camera;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Camera Activation
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Activate a camera and its audio listener.
    /// Ensures only one camera and one audio listener are active.
    /// </summary>
    private void ActivateCamera(Camera i_camera, AudioListener i_audioListener) {
        if (i_camera == null) return;

        // Disable ALL audio listeners first (prevents conflicts)
        DisableAllAudioListeners();

        // Enable camera
        i_camera.enabled = true;

        // Enable audio listener if it exists
        if (i_audioListener != null) {
            i_audioListener.enabled = true;
            _activeAudioListener = i_audioListener;
            Log($"Audio listener enabled on: {i_camera.name}");
        } else {
            LogWarning($"Camera {i_camera.name} has no AudioListener component");
        }

        Log($"Camera activated: {i_camera.name}");
    }

    /// <summary>
    /// Deactivate a camera and its audio listener.
    /// </summary>
    private void DeactivateCamera(Camera i_camera) {
        if (i_camera == null) return;

        i_camera.enabled = false;

        AudioListener audioListener = i_camera.GetComponent<AudioListener>();
        if (audioListener != null) {
            audioListener.enabled = false;
        }

        Log($"Camera deactivated: {i_camera.name}");
    }

    /// <summary>
    /// Disable all audio listeners to prevent conflicts.
    /// Called before activating a new camera.
    /// </summary>
    private void DisableAllAudioListeners() {
        // Disable bootstrap audio listener
        if (_bootstrapAudioListener != null) {
            _bootstrapAudioListener.enabled = false;
        }

        // Disable all scene camera audio listeners
        foreach (var camera in _registeredSceneCameras) {
            if (camera != null) {
                AudioListener listener = camera.GetComponent<AudioListener>();
                if (listener != null) {
                    listener.enabled = false;
                }
            }
        }

        _activeAudioListener = null;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Query Methods
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Get the currently active camera.
    /// </summary>
    public Camera GetActiveCamera() {
        return _ActiveCamera;
    }

    /// <summary>
    /// Check if bootstrap camera is currently active.
    /// </summary>
    public bool IsBootstrapCameraActive() {
        return _activeSceneCamera == null && _bootstrapCamera != null && _bootstrapCamera.enabled;
    }

    /// <summary>
    /// Check if a scene camera is currently active.
    /// </summary>
    public bool IsSceneCameraActive() {
        return _activeSceneCamera != null && _activeSceneCamera.enabled;
    }

    /// <summary>
    /// Get the number of registered scene cameras.
    /// </summary>
    public int GetSceneCameraCount() {
        return _registeredSceneCameras.Count;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    private void Log(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.Log($"[CameraManager] {i_message}");
        }
    }

    private void LogWarning(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.LogWarning($"[CameraManager] {i_message}");
        }
    }

    private void LogError(string i_message) {
        Debug.LogError($"[CameraManager] {i_message}");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Editor Utilities
    ////////////////////////////////////////////////////////////

#if UNITY_EDITOR
    [ContextMenu("Debug: Show Active Camera")]
    private void DebugShowActiveCamera() {
        if (_activeSceneCamera != null) {
            Debug.Log($"[CameraManager] Active: Scene Camera '{_activeSceneCamera.name}'");
        } else if (_bootstrapCamera != null) {
            Debug.Log($"[CameraManager] Active: Bootstrap Camera '{_bootstrapCamera.name}'");
        } else {
            Debug.Log("[CameraManager] No active camera");
        }
    }

    [ContextMenu("Debug: List Registered Cameras")]
    private void DebugListRegisteredCameras() {
        Debug.Log($"[CameraManager] Registered scene cameras: {_registeredSceneCameras.Count}");
        foreach (var camera in _registeredSceneCameras) {
            if (camera != null) {
                Debug.Log($"  - {camera.name} (enabled: {camera.enabled})");
            }
        }
    }
#endif

    #endregion
}