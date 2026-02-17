using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ScriptableObject manager for the player
/// Acts as the bridge between the game manager system and the player instance
/// Manages player initialization and coordinates update timing
/// </summary>
[CreateAssetMenu(fileName = "PlayerManager", menuName = "ScriptableObjects/Managers/PlayerManager")]
public class PlayerManager : ScriptableObject, IInitializable, IUpdateable, IFixedUpdateable {
    [Header("Configuration")]
    [SerializeField] private PlayerDefaultStatsSO _defaultStats;

    [Header("Runtime References")]
    private PlayerHandler _playerHandler;
    private bool _isInitialized = false;

    [Header("Debug Settings")]
    [SerializeField] private bool _iEnableDebugLogs = false;

    public string _ManagerName => GetType().Name;

    #region Manager Lifecycle

    public async Task Initialize() {
        if (_defaultStats == null) {
            LogError($"PlayerDefaultStatsSO not assigned!");
        }
        await Task.CompletedTask;
    }

    public void OnUpdate() {
        if (!_isInitialized || _playerHandler == null) return;

        // Delegate to PlayerHandler
        _playerHandler.OnUpdate();
    }

    public void OnFixedUpdate() {
        if (!_isInitialized || _playerHandler == null) return;

        // Delegate to PlayerHandler
        _playerHandler.OnFixedUpdate();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Register the player handler instance (called by PlayerHandler.Awake)
    /// </summary>
    public void SetPlayerHandler(PlayerHandler handler) {
        if (_playerHandler != null && _playerHandler != handler) {
            LogWarning($"PlayerHandler already registered! Replacing...");
        }

        _playerHandler = handler;
        Log($"PlayerHandler registered");

        // Initialize the player handler now that it's registered
        _playerHandler.Initialize(_defaultStats);
        _isInitialized = true;
        Log($"Initialized successfully");
    }

    /// <summary>
    /// Get the current player handler instance
    /// </summary>
    public PlayerHandler GetPlayerHandler() {
        return _playerHandler;
    }

    /// <summary>
    /// Check if player is initialized and active
    /// </summary>
    public bool IsPlayerActive() {
        return _isInitialized && _playerHandler != null;
    }

    /// <summary>
    /// Get player position (convenience method)
    /// </summary>
    public Vector3 GetPlayerPosition() {
        return _playerHandler != null ? _playerHandler.GetPosition() : Vector3.zero;
    }

    /// <summary>
    /// Enable/disable player control (convenience method)
    /// </summary>
    public void SetPlayerControlEnabled(bool enabled) {
        _playerHandler?.SetControlEnabled(enabled);
    }

    #endregion

    #region Cleanup

    private void OnDisable() {
        // Clear runtime references when ScriptableObject is unloaded
        _playerHandler = null;
        _isInitialized = false;
    }

    #endregion

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