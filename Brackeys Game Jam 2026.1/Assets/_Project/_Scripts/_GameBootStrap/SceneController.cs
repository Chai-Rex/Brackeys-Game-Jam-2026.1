using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles all scene loading, unloading, and container management.
/// </summary>
public class SceneController {

    private readonly Dictionary<string, SceneContainerSO> _containerMap = new Dictionary<string, SceneContainerSO>();
    private SceneContainerSO _activeContainer;

    // Events for external systems to listen to
    public event Action<string> _OnSceneLoadStarted;
    public event Action<string> _OnSceneLoadCompleted;
    public event Action<string> _OnSceneUnloadStarted;
    public event Action<string> _OnSceneUnloadCompleted;

    private readonly bool _enableDebugLogs;

    ////////////////////////////////////////////////////////////
    // Constructor with optional debug logging
    public SceneController(bool enableDebugLogs = false) {
        _enableDebugLogs = enableDebugLogs;
    }

    ////////////////////////////////////////////////////////////
    #region Container Registration
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Register a scene container for later loading
    /// </summary>
    public void RegisterContainer(SceneContainerSO container) {
        if (container == null) {
            LogWarning("Attempted to register null container");
            return;
        }

        string sceneName = container.GetSceneName();

        if (_containerMap.ContainsKey(sceneName)) {
            LogWarning($"Container for scene '{sceneName}' already registered");
            return;
        }

        _containerMap[sceneName] = container;
        Log($"Registered container: {sceneName}");
    }

    /// <summary>
    /// Register multiple containers at once
    /// </summary>
    public void RegisterContainers(params SceneContainerSO[] containers) {
        foreach (var container in containers) {
            RegisterContainer(container);
        }
    }

    /// <summary>
    /// Check if a scene has a registered container
    /// </summary>
    public bool HasContainer(string sceneName) {
        return _containerMap.ContainsKey(sceneName);
    }

    /// <summary>
    /// Get a registered container by scene name
    /// </summary>
    public SceneContainerSO GetContainer(string sceneName) {
        return _containerMap.TryGetValue(sceneName, out var container) ? container : null;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Scene Loading
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Start loading a scene and return the AsyncOperation immediately
    /// Call FinalizeSceneLoad() after tracking progress
    /// </summary>
    public AsyncOperation StartLoadingScene(string sceneName, out SceneContainerSO container) {
        container = null;

        if (!_containerMap.TryGetValue(sceneName, out container)) {
            LogError($"No container registered for scene '{sceneName}'");
            return null;
        }

        return StartLoadingContainer(container);
    }

    /// <summary>
    /// Start loading a scene container, return AsyncOperation immediately
    /// </summary>
    public AsyncOperation StartLoadingContainer(SceneContainerSO container) {
        if (container == null) {
            LogError("Cannot load null scene container");
            return null;
        }

        string sceneName = container.GetSceneName();
        Log($"Starting scene load: {sceneName}");
        _OnSceneLoadStarted?.Invoke(sceneName);

        // Start loading Unity scene (don't await)
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        return sceneLoad;
    }

    /// <summary>
    /// Finalize scene loading after AsyncOperation completes
    /// </summary>
    public async Task FinalizeSceneLoad(SceneContainerSO container, AsyncOperation operation) {
        if (container == null || operation == null) return;

        string sceneName = container.GetSceneName();

        // Unload previous scene
        if (_activeContainer != null) {
            await UnloadSceneContainer(_activeContainer);
        }

        // Wait for operation to complete (should already be done)
        if (!operation.isDone) {
            await operation;
        }

        // Initialize container
        await container.Initialize();

        _activeContainer = container;
        _OnSceneLoadCompleted?.Invoke(sceneName);
        Log($"Scene loaded: {sceneName}");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Scene Unloading
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Unload a scene container
    /// </summary>
    public async Task UnloadSceneContainer(SceneContainerSO container) {
        if (container == null) {
            LogWarning("Cannot unload null container");
            return;
        }

        if (!container.IsInitialized()) {
            LogWarning($"Container for {container.GetSceneName()} not initialized");
            return;
        }

        string sceneName = container.GetSceneName();
        Log($"Unloading scene: {sceneName}");
        _OnSceneUnloadStarted?.Invoke(sceneName);

        try {
            // Cleanup container
            container.Unload();

            // Unload Unity scene
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded) {
                AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(sceneName);
                if (sceneUnload != null) {
                    await sceneUnload;
                }
            }

            // Clear active reference
            if (_activeContainer == container) {
                _activeContainer = null;
            }

            Log($"Scene unloaded: {sceneName}");
            _OnSceneUnloadCompleted?.Invoke(sceneName);
        } catch (Exception e) {
            LogError($"Failed to unload scene {sceneName}: {e.Message}");
        }
    }

    /// <summary>
    /// Unload the currently active scene
    /// </summary>
    public async Task UnloadActiveScene() {
        if (_activeContainer != null) {
            await UnloadSceneContainer(_activeContainer);
        }
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Update Management
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Forward Update to active container
    /// </summary>
    public void Update() {
        _activeContainer?.Update();
    }

    /// <summary>
    /// Forward FixedUpdate to active container
    /// </summary>
    public void FixedUpdate() {
        _activeContainer?.FixedUpdate();
    }

    /// <summary>
    /// Pause the active scene
    /// </summary>
    public void Pause() {
        _activeContainer?.Pause();
    }

    /// <summary>
    /// Resume the active scene
    /// </summary>
    public void Resume() {
        _activeContainer?.Resume();
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Query Methods
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Get the currently active scene container
    /// </summary>
    public SceneContainerSO GetActiveContainer() => _activeContainer;

    /// <summary>
    /// Get the name of the currently active scene
    /// </summary>
    public string GetActiveSceneName() => _activeContainer?.GetSceneName();

    /// <summary>
    /// Check if a specific scene is currently loaded
    /// </summary>
    public bool IsSceneLoaded(string sceneName) {
        return _activeContainer != null && _activeContainer.GetSceneName() == sceneName;
    }

    /// <summary>
    /// Check if any scene is currently loaded
    /// </summary>
    public bool HasActiveScene() => _activeContainer != null;

    #endregion

    ////////////////////////////////////////////////////////////
    #region Cleanup
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Cleanup all scenes and clear registrations
    /// </summary>
    public async Task Cleanup() {
        Log("Cleaning up SceneController...");

        if (_activeContainer != null) {
            await UnloadSceneContainer(_activeContainer);
        }

        _containerMap.Clear();
        Log("SceneController cleanup complete");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    private void Log(string message) {
        if (_enableDebugLogs) {
            Debug.Log($"[SceneController] {message}");
        }
    }

    private void LogWarning(string message) {
        if (_enableDebugLogs) {
            Debug.LogWarning($"[SceneController] {message}");
        }
    }

    private void LogError(string message) {
        Debug.LogError($"[SceneController] {message}");
    }

    #endregion
}