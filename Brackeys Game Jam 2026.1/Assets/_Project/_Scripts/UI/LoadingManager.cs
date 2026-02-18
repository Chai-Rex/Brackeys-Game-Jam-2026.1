using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UtilityExtensions;

/// <summary>
/// Manages loading screen display and progress tracking.
/// </summary>
[CreateAssetMenu(fileName = "LoadingManager", menuName = "ScriptableObjects/Managers/LoadingManager")]
public class LoadingManager : ScriptableObject, IInitializable, ICleanable, IPersistentManager {

    [Header("Canvas Reference")]
    [SerializeField] private AssetReferenceGameObject _iLoadingCanvasPrefab;

    [Header("Settings")]
    [SerializeField] private bool _iEnableDebugLogs = false;

    // Runtime references
    private LoadingCanvas _loadingCanvas;
    private GameObject _loadingParent;
    private CancellationTokenSource _currentLoadingCts;

    // State
    private bool _isLoading = false;

    // Properties
    public string _ManagerName => GetType().Name;
    public bool IsLoading => _isLoading;

    ////////////////////////////////////////////////////////////
    #region Initialization
    ////////////////////////////////////////////////////////////

    public async Task Initialize() {

        Log("Initializing...");

        // Create parent GameObject
        _loadingParent = new GameObject("===== Loading Canvas =====");
        UnityEngine.Object.DontDestroyOnLoad(_loadingParent);

        // Load and instantiate loading canvas
        _loadingCanvas = await UnityAddressableExtensions.InstantiateAsync<LoadingCanvas>(
            _iLoadingCanvasPrefab,
            _loadingParent.transform
        );

        if (_loadingCanvas == null) {
            LogError("Failed to instantiate loading canvas");
            return;
        }

        // Hide by default
        _loadingCanvas.gameObject.SetActive(false);

        Log("Initialized successfully");
    }

    public void CleanUp() {

        Log("Cleaning up...");

        // Cancel any ongoing loading
        _currentLoadingCts?.Cancel();
        _currentLoadingCts?.Dispose();
        _currentLoadingCts = null;

        // Destroy canvas and parent
        if (_loadingCanvas != null) {
            Destroy(_loadingCanvas.gameObject);
            _loadingCanvas = null;
        }

        if (_loadingParent != null) {
            Destroy(_loadingParent);
            _loadingParent = null;
        }

        _isLoading = false;
        Log("Cleanup complete");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Loading Control
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Show loading screen with progress tracking
    /// </summary>
    /// <param name="i_taskName">Name of the task being loaded</param>
    /// <param name="i_targetProgress">Target progress (0-1)</param>
    /// <param name="i_operation">Optional Unity AsyncOperation to track</param>
    public async Task ShowLoading(string i_taskName, float i_targetProgress, AsyncOperation i_operation = null) {
        if (_loadingCanvas == null) {
            LogError("Loading canvas is null");
            return;
        }

        // Cancel any previous loading operation
        _currentLoadingCts?.Cancel();
        _currentLoadingCts = new CancellationTokenSource();

        _isLoading = true;

        try {
            // Show canvas
            _loadingCanvas.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;

            Log($"Loading: {i_taskName} ({i_targetProgress * 100:F0}%)");

            // Update loading canvas
            if (i_operation != null) {
                // Track AsyncOperation progress
                await _loadingCanvas.LoadWithOperation(i_taskName, i_operation, _currentLoadingCts.Token);
            } else {
                // Just animate to target progress
                await _loadingCanvas.LoadToProgress(i_taskName, i_targetProgress, _currentLoadingCts.Token);
            }

            // Hide canvas if we've reached 100%
            if (i_targetProgress >= 1f) {
                await HideLoading();
            }
        } catch (OperationCanceledException) {
            Log("Loading cancelled");
        } catch (Exception e) {
            LogError($"Error during loading: {e.Message}");
        } finally {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Show loading screen and immediately set to a specific progress
    /// (No animation, instant update)
    /// </summary>
    public void ShowLoadingInstant(string i_taskName, float i_progress) {
        if (_loadingCanvas == null) return;

        _loadingCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        _loadingCanvas.SetProgressInstant(i_taskName, i_progress);

        _isLoading = true;
        Log($"Loading (instant): {i_taskName} ({i_progress * 100:F0}%)");
    }

    /// <summary>
    /// Hide the loading screen
    /// </summary>
    public async Task HideLoading() {
        if (_loadingCanvas == null) return;

        // Optional: fade out animation here
        await Awaitable.WaitForSecondsAsync(.1f); // Small delay for visual polish

        _loadingCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;

        _isLoading = false;
        Log("Loading screen hidden");
    }

    /// <summary>
    /// Force hide loading screen immediately (no animation)
    /// </summary>
    public void HideLoadingInstant() {
        if (_loadingCanvas == null) return;

        _loadingCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;

        _isLoading = false;
        Log("Loading screen hidden (instant)");
    }

    /// <summary>
    /// Update loading text without changing progress
    /// </summary>
    public void UpdateLoadingText(string i_taskName) {
        if (_loadingCanvas == null) return;
        _loadingCanvas.UpdateText(i_taskName);
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Convenience Methods (Simplified API)
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Simplified method for backwards compatibility with your original API
    /// </summary>
    public async Task Load(string i_taskName, float i_percentage, AsyncOperation i_operation = null) {
        await ShowLoading(i_taskName, i_percentage, i_operation);
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    private void Log(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.Log($"[LoadingManager] {i_message}");
        }
    }

    private void LogWarning(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.LogWarning($"[LoadingManager] {i_message}");
        }
    }

    private void LogError(string i_message) {
        Debug.LogError($"[LoadingManager] {i_message}");
    }

    #endregion
}
