using SoundSystem;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Main entry point and bootstrap for the game.
/// Now includes CameraManager for handling bootstrap and scene cameras.
/// </summary>
public class GameBootstrap : MonoBehaviour {

    [Header("Persistent Managers")]
    [SerializeField] private LoadingManager _iLoadingManager;
    [SerializeField] private SceneCameraManager _iCameraManager;
    [SerializeField] private GameCommandsManager _iGameCommands;
    [SerializeField] private SoundManager _iSoundManager;
    [SerializeField] private InputManager _iInputManager;

    [Header("Scene Containers")]
    [SerializeField] private SceneContainerSO _iMainMenuContainer;
    [SerializeField] private SceneContainerSO _iGameplayContainer;

    [Header("Core GameObjects")]
    [SerializeField] private Camera _iCameraTemplate;
    [SerializeField] private EventSystem _iEventSystemTemplate;

    [Header("Debug Settings")]
    [SerializeField] private bool _iEnableDebugLogs = true;
    [SerializeField] private bool _iEnableVerboseLogs = false;

    // Helper systems
    private SceneController _sceneController;

    // Runtime references
    private Camera _camera;
    private EventSystem _eventSystem;
    private bool _canUpdate = false;

    // Public property for SceneCamera to access
    public SceneCameraManager CameraManager => _iCameraManager;

    ////////////////////////////////////////////////////////////
    #region Initialization
    ////////////////////////////////////////////////////////////

    private void Awake() {
        DontDestroyOnLoad(gameObject);

        // Create SceneController
        _sceneController = new SceneController(_iEnableDebugLogs && _iEnableVerboseLogs);

        RegisterSceneContainers();
        SubscribeToSceneEvents();
    }

    private void Start() {
        _ = StartAsync();
    }

    private async Task StartAsync() {
        Log("Starting initialization sequence...");

        try {
            await InitializePersistentSystems();
            await LoadInitialScene();

            ObserveInputManager();

            _canUpdate = true;

            Log("Initialization complete!");
        } catch (System.Exception e) {
            LogError($"Fatal error during initialization: {e.Message}", e);
        }
    }

    private void RegisterSceneContainers() {
        _sceneController.RegisterContainers(_iMainMenuContainer, _iGameplayContainer);
    }

    private void SubscribeToSceneEvents() {
        // Listen for scene loading events to show bootstrap camera
        _sceneController._OnSceneLoadStarted += HandleSceneLoadStarted;
    }

    private async Task InitializePersistentSystems() {
        Log("Initializing persistent systems...");

        // ========================================
        // STEP 1: Initialize LoadingManager first
        // ========================================
        await _iLoadingManager.Initialize();

        // ========================================
        // STEP 2: Initialize CameraManager
        // ========================================
        await _iCameraManager.Initialize();

        // ========================================
        // STEP 3: Create and register bootstrap camera
        // ========================================
        _camera = Instantiate(_iCameraTemplate);
        DontDestroyOnLoad(_camera.gameObject);
        _camera.name = "Bootstrap Camera";

        // Register with CameraManager (this activates it for loading screens)
        _iCameraManager.RegisterBootstrapCamera(_camera);

        // ========================================
        // STEP 4: Create EventSystem
        // ========================================
        _eventSystem = Instantiate(_iEventSystemTemplate);
        DontDestroyOnLoad(_eventSystem.gameObject);
        _eventSystem.name = "Bootstrap EventSystem";

        // ========================================
        // STEP 5: Initialize GameCommandsManager
        // ========================================
        await _iGameCommands.Initialize();
        _iGameCommands.RegisterBootstrap(this);

        // ========================================
        // STEP 6: Show loading screen
        // ========================================
        await _iLoadingManager.ShowLoading("Starting Game...", 0.1f);

        // ========================================
        // STEP 7: Initialize other managers
        // ========================================
        await _iLoadingManager.ShowLoading("Initializing Sound System", 0.3f);
        await _iSoundManager.Initialize();

        await _iLoadingManager.ShowLoading("Initializing Input System", 0.5f);
        await _iInputManager.Initialize();

        await _iLoadingManager.ShowLoading("Core Systems Ready", 0.7f);

        Log("Persistent systems initialized");
    }

    private async Task LoadInitialScene() {
        await LoadSceneContainer(_iMainMenuContainer);
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Scene Loading
    ////////////////////////////////////////////////////////////

    private async Task LoadSceneContainer(SceneContainerSO i_container) {
        if (i_container == null) {
            LogError("Cannot load null scene container");
            return;
        }

        string sceneName = i_container.GetSceneName();
        Log($"Loading scene: {sceneName}");

        // ========================================
        // STEP 1: Switch to bootstrap camera (for loading screen)
        // ========================================
        _iCameraManager.ShowBootstrapCamera();

        // ========================================
        // STEP 2: Start loading scene
        // ========================================
        AsyncOperation sceneLoad = _sceneController.StartLoadingContainer(i_container);

        if (sceneLoad == null) {
            LogError($"Failed to start loading scene: {sceneName}");
            return;
        }

        // ========================================
        // STEP 3: Track progress with LoadingManager
        // ========================================
        await _iLoadingManager.ShowLoading($"Loading {sceneName}", 1f, sceneLoad);

        // ========================================
        // STEP 4: Finalize scene load
        // ========================================
        await _sceneController.FinalizeSceneLoad(i_container, sceneLoad);

        // ========================================
        // STEP 5: Scene camera will auto-register via SceneCamera component
        // CameraManager will automatically switch to it
        // ========================================

        Log($"Scene loaded: {sceneName}");
    }

    private async Task TransitionToScene(string i_sceneName) {
        _canUpdate = false;

        try {
            SceneContainerSO container = _sceneController.GetContainer(i_sceneName);
            if (container == null) {
                LogError($"No container registered for scene '{i_sceneName}'");
                return;
            }

            await LoadSceneContainer(container);
        } finally {
            _canUpdate = true;
        }
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Scene Events
    ////////////////////////////////////////////////////////////

    private void HandleSceneLoadStarted(string i_sceneName) {
        // Ensure bootstrap camera is active when loading starts
        _iCameraManager.ShowBootstrapCamera();
        LogVerbose($"Bootstrap camera activated for loading: {i_sceneName}");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Unity Lifecycle
    ////////////////////////////////////////////////////////////

    private void OnDestroy() {
        Log("Shutting down...");

        UnsubscribeFromInputManager();
        UnsubscribeFromSceneEvents();

        // Cleanup systems
        _ = _sceneController.Cleanup();
        _iLoadingManager.CleanUp();
        _iCameraManager.CleanUp();
        _iGameCommands.CleanUp();
        _iSoundManager.CleanUp();
        _iInputManager.CleanUp();

        Log("Shutdown complete");
    }

    private void UnsubscribeFromSceneEvents() {
        if (_sceneController != null) {
            _sceneController._OnSceneLoadStarted -= HandleSceneLoadStarted;
        }
    }

    private void Update() {
        if (!_canUpdate) return;
        _sceneController?.Update();
    }

    private void FixedUpdate() {
        if (!_canUpdate) return;
        _sceneController?.FixedUpdate();
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Input Handling
    ////////////////////////////////////////////////////////////

    private void ObserveInputManager() {
        if (_iInputManager == null) {
            LogWarning("InputManager is null");
            return;
        }

        _iInputManager._PlayerPauseAction.started += OnPlayerPausePressed;
        _iInputManager._UIResumeAction.started += OnUIResumePressed;
        LogVerbose("Subscribed to input events");
    }

    private void UnsubscribeFromInputManager() {
        if (_iInputManager == null) return;

        _iInputManager._PlayerPauseAction.started -= OnPlayerPausePressed;
        _iInputManager._UIResumeAction.started -= OnUIResumePressed;
        LogVerbose("Unsubscribed from input events");
    }

    private void OnPlayerPausePressed(UnityEngine.InputSystem.InputAction.CallbackContext context) {
        PauseGame();
    }

    private void OnUIResumePressed(UnityEngine.InputSystem.InputAction.CallbackContext context) {
        ResumeGame();
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Public API (Called by GameCommandsManager)
    ////////////////////////////////////////////////////////////

    public async void BeginGame() {
        Log("Starting gameplay");
        await TransitionToScene(_iGameplayContainer.GetSceneName());
    }

    public async void ReturnToMainMenu() {
        Log("Returning to main menu");
        await TransitionToScene(_iMainMenuContainer.GetSceneName());
    }

    public async void LoadScene(string i_sceneName) {
        await TransitionToScene(i_sceneName);
    }

    public void PauseGame() {
        LogVerbose("Pausing game");
        _iInputManager.SetUIActionMap();
        _sceneController.Pause();
    }

    public void ResumeGame() {
        LogVerbose("Resuming game");
        _iInputManager.SetPlayerActionMap();
        _sceneController.Resume();
    }

    public SceneContainerSO GetActiveContainer() => _sceneController.GetActiveContainer();

    public bool IsSceneLoaded(string i_sceneName) => _sceneController.IsSceneLoaded(i_sceneName);

    #endregion

    ////////////////////////////////////////////////////////////
    #region Debug Logging
    ////////////////////////////////////////////////////////////

    private void Log(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.Log($"[GameBootstrap] {i_message}");
        }
    }

    private void LogVerbose(string i_message) {
        if (_iEnableDebugLogs && _iEnableVerboseLogs) {
            Debug.Log($"[GameBootstrap] {i_message}");
        }
    }

    private void LogWarning(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.LogWarning($"[GameBootstrap] {i_message}");
        }
    }

    private void LogError(string i_message, System.Exception i_exception = null) {
        if (i_exception != null) {
            Debug.LogError($"[GameBootstrap] {i_message}\n{i_exception.Message}\n{i_exception.StackTrace}");
        } else {
            Debug.LogError($"[GameBootstrap] {i_message}");
        }
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Editor Debug Tools
    ////////////////////////////////////////////////////////////

#if UNITY_EDITOR
    [ContextMenu("Debug: Current Scene & Camera")]
    private void DebugCurrentSceneAndCamera() {
        Debug.Log($"[GameBootstrap] Active scene: {_sceneController?.GetActiveSceneName() ?? "None"}");
        Debug.Log($"[GameBootstrap] Active camera: {_iCameraManager?.GetActiveCamera()?.name ?? "None"}");
        Debug.Log($"[GameBootstrap] Using bootstrap camera: {_iCameraManager?.IsBootstrapCameraActive()}");
    }

    [ContextMenu("Debug: Force Load Main Menu")]
    private async void DebugForceLoadMainMenu() {
        await TransitionToScene(_iMainMenuContainer.GetSceneName());
    }

    [ContextMenu("Debug: Force Load Gameplay")]
    private async void DebugForceLoadGameplay() {
        await TransitionToScene(_iGameplayContainer.GetSceneName());
    }

#endif

    #endregion
}