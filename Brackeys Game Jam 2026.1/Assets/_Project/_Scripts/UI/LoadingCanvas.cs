using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component for the loading screen.
/// Handles progress bar animation and text display.
/// </summary>
public class LoadingCanvas : MonoBehaviour {

    [Header("UI Elements")]
    [SerializeField] private Image _progressImage;
    [SerializeField] private TMP_Text _taskText;

    [Header("Animation Settings")]
    [SerializeField] private float _animationSpeed = 3f;
    [SerializeField] private bool _smoothAnimation = true;

    [Header("Debug")]
    [SerializeField] private bool _enableDebugLogs = false;

    ////////////////////////////////////////////////////////////
    #region Loading Methods
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Animate progress bar to a specific target value
    /// </summary>
    public async Task LoadToProgress(string taskName, float targetProgress, CancellationToken cancellationToken = default) {
        if (_progressImage == null || _taskText == null) {
            Debug.LogError("[LoadingCanvas] UI elements not assigned!");
            return;
        }

        // Clamp target progress
        targetProgress = Mathf.Clamp01(targetProgress);

        // Update text
        UpdateText(taskName);
        Log($"Loading: {taskName} - {targetProgress * 100:F0}%");

        // Animate progress bar
        if (_smoothAnimation) {
            while (_progressImage.fillAmount < targetProgress && !cancellationToken.IsCancellationRequested) {
                _progressImage.fillAmount = Mathf.MoveTowards(
                    _progressImage.fillAmount,
                    targetProgress,
                    _animationSpeed * Time.deltaTime
                );
                await Task.Yield();
            }
        } else {
            _progressImage.fillAmount = targetProgress;
        }

        // Ensure we hit exact target
        if (!cancellationToken.IsCancellationRequested) {
            _progressImage.fillAmount = targetProgress;
        }
    }

    /// <summary>
    /// Track a Unity AsyncOperation and animate progress accordingly
    /// </summary>
    public async Task LoadWithOperation(string taskName, AsyncOperation operation, CancellationToken cancellationToken = default) {
        if (_progressImage == null || _taskText == null) {
            Debug.LogError("[LoadingCanvas] UI elements not assigned!");
            return;
        }

        if (operation == null) {
            Debug.LogWarning("[LoadingCanvas] AsyncOperation is null, falling back to instant progress");
            await LoadToProgress(taskName, 1f, cancellationToken);
            return;
        }

        // Update text
        UpdateText(taskName);
        Log($"Loading with AsyncOperation: {taskName}");

        // Track operation progress
        // Note: Unity's AsyncOperation.progress caps at 0.9 until isDone
        while (!operation.isDone && !cancellationToken.IsCancellationRequested) {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f); // Normalize to 0-1

            if (_smoothAnimation) {
                _progressImage.fillAmount = Mathf.MoveTowards(
                    _progressImage.fillAmount,
                    targetProgress,
                    _animationSpeed * Time.deltaTime
                );
            } else {
                _progressImage.fillAmount = targetProgress;
            }

            await Task.Yield();
        }

        // Animate to 100% when operation completes
        if (!cancellationToken.IsCancellationRequested) {
            await AnimateToFull(cancellationToken);
        }
    }

    /// <summary>
    /// Animate progress bar from current value to 100%
    /// </summary>
    private async Task AnimateToFull(CancellationToken cancellationToken = default) {
        if (_smoothAnimation) {
            while (_progressImage.fillAmount < 1f && !cancellationToken.IsCancellationRequested) {
                _progressImage.fillAmount = Mathf.MoveTowards(
                    _progressImage.fillAmount,
                    1f,
                    _animationSpeed * Time.deltaTime
                );
                await Task.Yield();
            }
        }

        // Ensure we hit exactly 100%
        if (!cancellationToken.IsCancellationRequested) {
            _progressImage.fillAmount = 1f;
        }
    }

    /// <summary>
    /// Set progress instantly without animation
    /// </summary>
    public void SetProgressInstant(string taskName, float progress) {
        if (_progressImage == null || _taskText == null) {
            Debug.LogError("[LoadingCanvas] UI elements not assigned!");
            return;
        }

        UpdateText(taskName);
        _progressImage.fillAmount = Mathf.Clamp01(progress);
        Log($"Progress set (instant): {taskName} - {progress * 100:F0}%");
    }

    /// <summary>
    /// Update only the task text
    /// </summary>
    public void UpdateText(string taskName) {
        if (_taskText != null) {
            _taskText.text = taskName;
        }
    }

    /// <summary>
    /// Reset progress bar to 0
    /// </summary>
    public void ResetProgress() {
        if (_progressImage != null) {
            _progressImage.fillAmount = 0f;
        }
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Unity Lifecycle
    ////////////////////////////////////////////////////////////

    private void OnEnable() {
        // Reset progress when canvas is shown
        ResetProgress();
        Log("Loading canvas enabled");
    }

    private void OnDisable() {
        Log("Loading canvas disabled");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    private void Log(string message) {
        if (_enableDebugLogs) {
            Debug.Log($"[LoadingCanvas] {message}");
        }
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Validation
    ////////////////////////////////////////////////////////////

#if UNITY_EDITOR
    private void OnValidate() {
        // Ensure animation speed is positive
        if (_animationSpeed <= 0f) {
            _animationSpeed = 1f;
            Debug.LogWarning("[LoadingCanvas] Animation speed must be positive, reset to 1");
        }

        // Warn if UI elements are not assigned
        if (_progressImage == null) {
            Debug.LogWarning("[LoadingCanvas] Progress Image not assigned!", this);
        }
        if (_taskText == null) {
            Debug.LogWarning("[LoadingCanvas] Task Text not assigned!", this);
        }
    }
#endif

    #endregion
}