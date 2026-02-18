using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

////////////////////////////////////////////////////////////
/// BASE MANAGER CONTAINER
////////////////////////////////////////////////////////////

public abstract class BaseManagerContainerSO : ScriptableObject,
    IInitializable, ICleanable, IUpdateable, IFixedUpdateable, IPausable {

    [Header("Managers")]
    [SerializeField] protected ScriptableObject[] _managers;
    [Tooltip("Can contain individual managers OR nested containers")]

    [Header("Debug Settings")]
    [SerializeField] protected bool _enableDebugLogs = false;

    // Cached interface lists (built once at initialization)
    protected List<IInitializable> _initializables;
    protected List<ICleanable> _cleanables;
    protected List<IUpdateable> _updateables;
    protected List<IFixedUpdateable> _fixedUpdateables;
    protected List<IPausable> _pausables;

    // Manager cache for GetManager<T>() calls
    protected Dictionary<Type, IManager> _managerCache;

    // State

    protected bool _isPaused = false;

    // Properties

    public virtual string ManagerName => GetType().Name;

    public string _ManagerName => GetType().Name;

    ////////////////////////////////////////////////////////////
    #region Initialization
    ////////////////////////////////////////////////////////////

    public virtual async Task Initialize() {

        Log("Initializing container...");

        // Cache interface references (one-time cost)
        CacheManagerInterfaces();

        // Initialize managers (skip persistent ones in scene containers)
        await InitializeManagers();

        Log("Initialization complete");
    }

    protected virtual async Task InitializeManagers() {
        foreach (var initializer in _initializables) {
            // Skip managers that should be initialized by GameBootstrap
            if (ShouldSkipInitialization(initializer)) {
                Log($"Skipping initialization: {initializer._ManagerName}");
                continue;
            }

            try {
                await initializer.Initialize();
                Log($"Initialized: {initializer._ManagerName}");
            } catch (Exception e) {
                LogError($"Failed to initialize {initializer._ManagerName}: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Override in derived classes to skip certain managers.
    /// Example: SceneContainer skips IPersistentManager
    /// </summary>
    protected virtual bool ShouldSkipInitialization(IInitializable initializer) {
        return initializer is IPersistentManager;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Interface Caching
    ////////////////////////////////////////////////////////////

    protected virtual void CacheManagerInterfaces() {
        _initializables = new List<IInitializable>();
        _cleanables = new List<ICleanable>();
        _updateables = new List<IUpdateable>();
        _fixedUpdateables = new List<IFixedUpdateable>();
        _pausables = new List<IPausable>();
        _managerCache = new Dictionary<Type, IManager>();

        foreach (var manager in _managers) {
            if (manager == null) {
                LogWarning("Null manager reference in container");
                continue;
            }

            // Cache interfaces
            if (manager is IInitializable init) _initializables.Add(init);
            if (manager is ICleanable clean) _cleanables.Add(clean);
            if (manager is IUpdateable upd) _updateables.Add(upd);
            if (manager is IFixedUpdateable fix) _fixedUpdateables.Add(fix);
            if (manager is IPausable pause) _pausables.Add(pause);

            // Cache manager by its concrete type for GetManager<T>()
            if (manager is IManager iManager) {
                _managerCache[manager.GetType()] = iManager;
            }
        }

        Log($"Cached: {_initializables.Count} initializable, " +
            $"{_cleanables.Count} cleanable, " +
            $"{_updateables.Count} updateable, " +
            $"{_fixedUpdateables.Count} fixed updateable, " +
            $"{_pausables.Count} pausable");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Update Loop
    ////////////////////////////////////////////////////////////

    public virtual void OnUpdate() {
        if (_isPaused) return;

        for (int i = 0; i < _updateables.Count; i++) {
            try {
                _updateables[i].OnUpdate();
            } catch (Exception e) {
                LogError($"Error in {_updateables[i]._ManagerName}.OnUpdate(): {e.Message}");
            }
        }
    }

    public virtual void OnFixedUpdate() {
        if (_isPaused) return;

        for (int i = 0; i < _fixedUpdateables.Count; i++) {
            try {
                _fixedUpdateables[i].OnFixedUpdate();
            } catch (Exception e) {
                LogError($"Error in {_fixedUpdateables[i]._ManagerName}.OnFixedUpdate(): {e.Message}");
            }
        }
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Pause/Resume
    ////////////////////////////////////////////////////////////

    public virtual void OnPause() {
        if (_isPaused) return;

        _isPaused = true;
        for (int i = 0; i < _pausables.Count; i++) {
            try {
                _pausables[i].OnPause();
            } catch (Exception e) {
                LogError($"Error in {_pausables[i]._ManagerName}.OnPause(): {e.Message}");
            }
        }

        Log("Paused");
    }

    public virtual void OnResume() {
        if (!_isPaused) return;

        _isPaused = false;
        for (int i = 0; i < _pausables.Count; i++) {
            try {
                _pausables[i].OnResume();
            } catch (Exception e) {
                LogError($"Error in {_pausables[i]._ManagerName}.OnResume(): {e.Message}");
            }
        }

        Log("Resumed");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Cleanup
    ////////////////////////////////////////////////////////////

    public virtual void CleanUp() {

        Log("Cleaning up container...");

        // Cleanup in reverse order
        for (int i = _cleanables.Count - 1; i >= 0; i--) {
            // Skip persistent managers in scene containers
            if (ShouldSkipCleanup(_cleanables[i])) {
                Log($"Skipping cleanup: {_cleanables[i]._ManagerName}");
                continue;
            }

            try {
                _cleanables[i].CleanUp();
                Log($"Cleaned up: {_cleanables[i]._ManagerName}");
            } catch (Exception e) {
                LogError($"Failed to cleanup {_cleanables[i]._ManagerName}: {e.Message}");
            }
        }

        // Clear cached lists
        _initializables?.Clear();
        _cleanables?.Clear();
        _updateables?.Clear();
        _fixedUpdateables?.Clear();
        _pausables?.Clear();
        _managerCache?.Clear();

        _isPaused = false;

        Log("Cleanup complete");
    }

    /// <summary>
    /// Override in derived classes to skip certain cleanups.
    /// Example: SceneContainer skips IPersistentManager
    /// </summary>
    protected virtual bool ShouldSkipCleanup(ICleanable cleanable) {
        return false;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Manager Access (Supports Nested Containers)
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Get a manager by its type. Searches nested containers recursively.
    /// Example: var playerManager = container.GetManager<PlayerManager>();
    /// </summary>
    public T GetManager<T>() where T : class, IManager {
        // Check cache first (direct managers only)
        if (_managerCache != null && _managerCache.TryGetValue(typeof(T), out IManager cachedManager)) {
            return cachedManager as T;
        }

        // Search direct managers
        T manager = GetDirectManager<T>();
        if (manager != null) {
            // Cache it
            if (_managerCache != null) {
                _managerCache[typeof(T)] = manager;
            }
            return manager;
        }

        // Search nested containers recursively
        manager = GetManagerFromNestedContainers<T>();
        if (manager != null) {
            // Cache it
            if (_managerCache != null) {
                _managerCache[typeof(T)] = manager;
            }
            return manager;
        }

        LogWarning($"Manager of type {typeof(T).Name} not found in container or nested containers");
        return null;
    }

    /// <summary>
    /// Get a manager directly from this container (no nested search)
    /// </summary>
    protected T GetDirectManager<T>() where T : class, IManager {
        foreach (var manager in _managers) {
            if (manager is T typedManager) {
                return typedManager;
            }
        }
        return null;
    }

    /// <summary>
    /// Search for a manager in nested containers recursively
    /// </summary>
    protected T GetManagerFromNestedContainers<T>() where T : class, IManager {
        foreach (var manager in _managers) {
            // Check if this manager is itself a container
            if (manager is BaseManagerContainerSO nestedContainer) {
                T foundManager = nestedContainer.GetManager<T>();
                if (foundManager != null) {
                    return foundManager;
                }
            }
        }
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
    /// Check if a manager exists in this container (including nested).
    /// Example: if (container.HasManager<PlayerManager>()) { ... }
    /// </summary>
    public bool HasManager<T>() where T : class, IManager {
        return GetManager<T>() != null;
    }

    /// <summary>
    /// Get all managers of a specific type (useful for nested containers)
    /// </summary>
    public List<T> GetAllManagers<T>() where T : class, IManager {
        List<T> results = new List<T>();

        // Get direct managers
        foreach (var manager in _managers) {
            if (manager is T typedManager) {
                results.Add(typedManager);
            }
        }

        // Get from nested containers
        foreach (var manager in _managers) {
            if (manager is BaseManagerContainerSO nestedContainer) {
                results.AddRange(nestedContainer.GetAllManagers<T>());
            }
        }

        return results;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Utility Methods
    ////////////////////////////////////////////////////////////

    public bool IsPaused() => _isPaused;

    public int GetManagerCount() => _managers?.Length ?? 0;

    public int GetDirectManagerCount() => _managers?.Length ?? 0;

    public int GetTotalManagerCount() {
        int count = 0;
        foreach (var manager in _managers) {
            count++;
            if (manager is BaseManagerContainerSO nestedContainer) {
                count += nestedContainer.GetTotalManagerCount();
            }
        }
        return count;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Validation (Editor Only)
    ////////////////////////////////////////////////////////////

#if UNITY_EDITOR
    protected virtual void OnValidate() {
        if (_managers == null) return;

        // Check for null references
        int nullCount = 0;
        foreach (var manager in _managers) {
            if (manager == null) nullCount++;
        }
        if (nullCount > 0) {
            LogWarning($"{nullCount} null manager references in container");
        }

        // Check for duplicate manager types (only direct managers)
        HashSet<Type> seenTypes = new HashSet<Type>();
        foreach (var manager in _managers) {
            if (manager == null) continue;
            if (manager is BaseManagerContainerSO) continue; // Skip nested containers

            Type managerType = manager.GetType();
            if (seenTypes.Contains(managerType)) {
                LogWarning($"Duplicate manager type: {managerType.Name}. This may cause issues with GetManager<T>()");
            }
            seenTypes.Add(managerType);
        }
    }
#endif

    #endregion

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    protected void Log(string message) {
        if (_enableDebugLogs) {
            Debug.Log($"[{ManagerName}] {message}");
        }
    }

    protected void LogWarning(string message) {
        if (_enableDebugLogs) {
            Debug.LogWarning($"[{ManagerName}] {message}");
        }
    }

    protected void LogError(string message) {
        Debug.LogError($"[{ManagerName}] {message}");
    }

    #endregion
}