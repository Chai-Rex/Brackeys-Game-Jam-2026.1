using UnityEngine;

/// <summary>
/// Component that designers add to cameras in scenes.
/// Automatically registers/unregisters the camera with CameraManager.
/// This ensures only one camera is active at a time (no Audio Listener conflicts).
/// </summary>
[AddComponentMenu("Game/Scene Camera")]
[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private SceneContainerSO _iSceneContainerSO;

    [Header("Info (do NOT set, visual only)")]
    [SerializeField] private bool _iIsRegistered = false;

    [Header("Debug Settings")]
    [SerializeField] private bool _iEnableDebugLogs = false;

    private Camera _camera;
    private SceneCameraManager _cameraManager;

    ////////////////////////////////////////////////////////////
    #region Unity Lifecycle
    ////////////////////////////////////////////////////////////

    private void Awake() {
        _camera = GetComponent<Camera>();

        GetCameraManager();

        RegisterWithManager();
    }

    private void OnDestroy() {
        UnregisterFromManager();
    }

    private void OnEnable() {
        // If already registered and being re-enabled, notify manager
        if (_iIsRegistered && _cameraManager != null) {
            _cameraManager.SwitchToSceneCamera(_camera);
        }
    }

    private void OnDisable() {
        // Don't unregister on disable - only on destroy
        // This allows designers to toggle cameras in editor
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Registration
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Initializes the camera manager by retrieving it from the scene container.
    /// </summary>
    private void GetCameraManager() {
        _cameraManager = _iSceneContainerSO.GetManager<SceneCameraManager>();
        if (_cameraManager == null) {
            LogError($"Could not find CameraManager in SceneContainerSO!");
            return;
        }
    }

    /// <summary>
    /// Manually register this camera with CameraManager.
    /// Called automatically if _autoRegister is true.
    /// </summary>
    public void RegisterWithManager() {
        if (_iIsRegistered) {
            LogWarning($"{gameObject.name} already registered");
            return;
        }

        if (_cameraManager == null) {
            LogError($"Could not find CameraManager!");
            return;
        }

        // Register this camera
        _cameraManager.RegisterSceneCamera(_camera);
        _iIsRegistered = true;

        Log($"Registered: {gameObject.name}");
    }

    /// <summary>
    /// Manually unregister this camera from CameraManager.
    /// Called automatically on OnDestroy.
    /// </summary>
    public void UnregisterFromManager() {
        if (!_iIsRegistered || _cameraManager == null) return;

        _cameraManager.UnregisterSceneCamera(_camera);
        _iIsRegistered = false;

        Log($"Unregistered: {gameObject.name}");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Editor Utilities
    ////////////////////////////////////////////////////////////

#if UNITY_EDITOR
    private void OnValidate() {
        // Ensure we have a Camera component
        if (GetComponent<Camera>() == null) {
            Debug.LogError($"[SceneCamera] {gameObject.name} requires a Camera component!", this);
        }
    }

    [ContextMenu("Force Register")]
    private void DebugForceRegister() {
        if (Application.isPlaying) {
            RegisterWithManager();
        } else {
            Debug.LogWarning("[SceneCamera] Can only register in Play Mode");
        }
    }

    [ContextMenu("Force Unregister")]
    private void DebugForceUnregister() {
        if (Application.isPlaying) {
            UnregisterFromManager();
        } else {
            Debug.LogWarning("[SceneCamera] Can only unregister in Play Mode");
        }
    }

    // Draw gizmo to show this is a scene camera
    private void OnDrawGizmos() {
        if (!_camera) _camera = GetComponent<Camera>();
        if (!_camera) return;

        Gizmos.color = _iIsRegistered ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Draw camera frustum
        if (_camera != null) {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, _camera.fieldOfView, _camera.farClipPlane, _camera.nearClipPlane, _camera.aspect);
        }
    }

    private void OnDrawGizmosSelected() {
        // Draw larger frustum when selected
        if (!_camera) _camera = GetComponent<Camera>();
        if (!_camera) return;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, _camera.fieldOfView, _camera.farClipPlane, _camera.nearClipPlane, _camera.aspect);
    }
#endif

    #endregion

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    private void Log(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.Log($"[SceneCamera] {i_message}");
        }
    }

    private void LogWarning(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.LogWarning($"[SceneCamera] {i_message}");
        }
    }

    private void LogError(string i_message) {
        Debug.LogError($"[SceneCamera] {i_message}");
    }

    #endregion
}