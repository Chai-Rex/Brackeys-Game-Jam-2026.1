using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

////////////////////////////////////////////////////////////
/// Scene Container with Cached Interface Lists
////////////////////////////////////////////////////////////

[CreateAssetMenu(fileName = "SceneContainerSO", menuName = "ScriptableObjects/Scene/SceneContainerSO")]
public class SceneContainerSO : ScriptableObject {

    [Header("Scene Info")]
    [SerializeField] private string _iSceneName;

    [Header("Managers")]
    [SerializeField] private ScriptableObject[] _iManagers;

    [Header("Debug Settings")]
    [SerializeField] private bool _iEnableDebugLogs = false;

    // Cached interface lists (built once at initialization)
    private List<IInitializable> _initializables;
    private List<IUpdateable> _updateables;
    private List<IFixedUpdateable> _fixedUpdateables;
    private List<IPausable> _pausables;

    // Manager cache for GetManager<T>() calls
    private Dictionary<Type, IManager> _managerCache;

    private bool _isInitialized = false;
    private bool _isPaused = false;

    ////////////////////////////////////////////////////////////
    /// Initialize and cache interface references
    ////////////////////////////////////////////////////////////
    public async Task Initialize() {
        if (_isInitialized) {
            LogWarning($"[{_iSceneName}] Already initialized");
            return;
        }

        Log($"[{_iSceneName}] Initializing scene container...");

        // Cache interface references (one-time cost)
        CacheManagerInterfaces();

        // Initialize only scene managers (skip persistent ones)
        foreach (var initializer in _initializables) {
            // Skip managers that should be initialized by GameBootstrap
            if (initializer is IPersistentManager) {
                Log($"[{_iSceneName}] Skipping persistent manager: {initializer._ManagerName}");
                continue;
            }

            try {
                await initializer.Initialize();
            } catch (Exception e) {
                LogError($"Failed to initialize {initializer._ManagerName}: {e.Message}\n{e.StackTrace}");
            }
        }

        _isInitialized = true;
        Log($"[{_iSceneName}] Initialization complete");
    }

    ////////////////////////////////////////////////////////////
    /// Cache all manager interfaces for fast access
    ////////////////////////////////////////////////////////////
    private void CacheManagerInterfaces() {
        _initializables = new List<IInitializable>();
        _updateables = new List<IUpdateable>();
        _fixedUpdateables = new List<IFixedUpdateable>();
        _pausables = new List<IPausable>();
        _managerCache = new Dictionary<Type, IManager>();

        foreach (var manager in _iManagers) {
            if (manager == null) {
                LogWarning($"[{_iSceneName}] Null manager reference in container");
                continue;
            }

            // Cache interfaces
            if (manager is IInitializable initialize) _initializables.Add(initialize);
            if (manager is IUpdateable update) _updateables.Add(update);
            if (manager is IFixedUpdateable fixedUpdate) _fixedUpdateables.Add(fixedUpdate);
            if (manager is IPausable pause) _pausables.Add(pause);

            // Cache manager by its concrete type for GetManager<T>()
            if (manager is IManager iManager) {
                _managerCache[manager.GetType()] = iManager;
            }
        }

        Log($"[{_iSceneName}] Cached: {_initializables.Count} initializable, " +
                 $"{_updateables.Count} updateable, {_fixedUpdateables.Count} fixed, " +
                 $"{_pausables.Count} pausable");
    }

    ////////////////////////////////////////////////////////////
    /// Direct interface calls - no virtual overhead
    ////////////////////////////////////////////////////////////
    public void Update() {
        if (!_isInitialized || _isPaused) return;

        for (int i = 0; i < _updateables.Count; i++) {
            _updateables[i].OnUpdate();
        }
    }

    public void FixedUpdate() {
        if (!_isInitialized || _isPaused) return;

        for (int i = 0; i < _fixedUpdateables.Count; i++) {
            _fixedUpdateables[i].OnFixedUpdate();
        }
    }

    public void Pause() {
        if (!_isInitialized || _isPaused) return;

        _isPaused = true;
        for (int i = 0; i < _pausables.Count; i++) {
            _pausables[i].OnPause();
        }
    }

    public void Resume() {
        if (!_isInitialized || !_isPaused) return;

        _isPaused = false;
        for (int i = 0; i < _pausables.Count; i++) {
            _pausables[i].OnResume();
        }
    }

    public void Unload() {
        if (!_isInitialized) return;

        Log($"[{_iSceneName}] Unloading scene container...");

        // Cleanup in reverse order, skip persistent managers
        for (int i = _initializables.Count - 1; i >= 0; i--) {
            if (_initializables[i] is IPersistentManager) {
                Log($"[{_iSceneName}] Skipping cleanup of persistent manager: {_initializables[i]._ManagerName}");
                continue;
            }

            try {
                _initializables[i].CleanUp();
            } catch (Exception e) {
                LogError($"Failed to cleanup {_initializables[i]._ManagerName}: {e.Message}");
            }
        }

        // Clear cached lists (but don't null them, just clear)
        _initializables?.Clear();
        _updateables?.Clear();
        _fixedUpdateables?.Clear();
        _pausables?.Clear();
        _managerCache?.Clear();

        _isInitialized = false;
        _isPaused = false;
    }

    ////////////////////////////////////////////////////////////
    /// Manager Access Methods
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Get a manager by its type. Cached for performance.
    /// Example: var playerManager = container.GetManager<PlayerManager>();
    /// </summary>
    public T GetManager<T>() where T : class, IManager {
        // Check cache first
        if (_managerCache != null && _managerCache.TryGetValue(typeof(T), out IManager cachedManager)) {
            return cachedManager as T;
        }

        // Fallback: search managers array
        foreach (var manager in _iManagers) {
            if (manager is T typedManager) {
                // Cache it for next time
                if (_managerCache != null) {
                    _managerCache[typeof(T)] = typedManager;
                }
                return typedManager;
            }
        }

        LogWarning($"[{_iSceneName}] Manager of type {typeof(T).Name} not found in container");
        return null;
    }

    /// <summary>
    /// Try to get a manager, returns true if found.
    /// Example: if (container.TryGetManager(out PlayerManager pm)) { ... }
    /// </summary>
    public bool TryGetManager<T>(out T manager) where T : class, IManager {
        manager = GetManager<T>();
        return manager != null;
    }

    /// <summary>
    /// Check if a manager exists in this container.
    /// Example: if (container.HasManager<PlayerManager>()) { ... }
    /// </summary>
    public bool HasManager<T>() where T : class, IManager {
        return GetManager<T>() != null;
    }

    ////////////////////////////////////////////////////////////
    /// Utility Methods
    ////////////////////////////////////////////////////////////
    public string GetSceneName() => _iSceneName;
    public bool IsInitialized() => _isInitialized;
    public bool IsPaused() => _isPaused;

    ////////////////////////////////////////////////////////////
    /// Validation (Editor Only)
    ////////////////////////////////////////////////////////////
#if UNITY_EDITOR
    private void OnValidate() {
        // Check for duplicate manager types
        HashSet<Type> seenTypes = new HashSet<Type>();
        foreach (var manager in _iManagers) {
            if (manager == null) continue;

            Type managerType = manager.GetType();
            if (seenTypes.Contains(managerType)) {
                LogWarning($"[{_iSceneName}] Duplicate manager type: {managerType.Name}. " +
                               $"This may cause issues with GetManager<T>()");
            }
            seenTypes.Add(managerType);
        }

        // Warn about persistent managers in scene containers
        //foreach (var manager in _managers) {
        //    if (manager is IPersistentManager persistent) {
        //        Debug.LogWarning($"[{_sceneName}] Found persistent manager '{manager.name}' in scene container. " +
        //                       $"Persistent managers should typically be initialized by GameBootstrap, not scene containers.", this);
        //    }
        //}
    }
#endif

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    private void Log(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.Log($"[SceneContainerSO] {i_message}");
        }
    }

    private void LogWarning(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.LogWarning($"[SceneContainerSO] {i_message}");
        }
    }

    private void LogError(string i_message) {
        Debug.LogError($"[SceneContainerSO] {i_message}");
    }

    #endregion
}
