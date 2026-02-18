using UnityEngine;

/// <summary>
/// Main MonoBehaviour that holds all player subsystems
/// Acts as a facade and delegates to specialized handlers
/// Update/FixedUpdate are called by PlayerManager to ensure proper timing with other managers
/// </summary>
public class PlayerHandler : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private SceneContainerSO _sceneContainer;

    [Header("Handler Components")]
    [SerializeField] private PlayerPhysicsHandler _physicsHandler;
    [SerializeField] private PlayerAnimamationHandler _animationHandler;
    [SerializeField] private PlayerStateMachineHandler _stateMachineHandler;
    [SerializeField] private PlayerCollisionHandler _collisionHandler;
    [SerializeField] private PlayerStatsHandler _statsHandler;
    [SerializeField] private PlayerBlackboardHandler _blackboardHandler;
    [SerializeField] private PlayerInputHandler _inputHandler;
    [SerializeField] private Transform _cameraFollowPlayerTransform;

    [Header("Debug Settings")]
    [SerializeField] private bool _iEnableDebugLogs = false;

    private PlayerManager _playerManager;
    private InputManager _inputManager;
    private bool _isInitialized = false;

    #region Public Accessors

    public SceneContainerSO SceneContainer => _sceneContainer;
    public PlayerPhysicsHandler Physics => _physicsHandler;
    public PlayerAnimamationHandler Animation => _animationHandler;
    public PlayerStateMachineHandler StateMachine => _stateMachineHandler;
    public PlayerCollisionHandler Collision => _collisionHandler;
    public PlayerStatsHandler Stats => _statsHandler;
    public PlayerBlackboardHandler Blackboard => _blackboardHandler;
    public PlayerInputHandler Input => _inputHandler;
    public Transform CameraFollowPlayer => _cameraFollowPlayerTransform;

    #endregion

    #region Unity Lifecycle

    private void Start() {
        ValidateComponents();
        RegisterWithManager();
    }

    // NOTE: Update and FixedUpdate are NOT used here!
    // They are called by PlayerManager via OnUpdate() and OnFixedUpdate()
    // This ensures timing is synchronized with other managers in the game

    #endregion

    #region Initialization

    private void ValidateComponents() {
        // Check if all required components are assigned
        if (_physicsHandler == null) LogError("PhysicsHandler not assigned!");
        if (_animationHandler == null) LogError("AnimationHandler not assigned!");
        if (_stateMachineHandler == null) LogError("StateMachineHandler not assigned!");
        if (_collisionHandler == null) LogError("CollisionHandler not assigned!");
        if (_statsHandler == null) LogError("StatsHandler not assigned!");
        if (_blackboardHandler == null) LogError("BlackboardHandler not assigned!");
        if (_inputHandler == null) LogError("InputHandler not assigned!");
        if (_cameraFollowPlayerTransform == null) LogError("CameraFollowPlayerTransform not assigned!");
    }

    private void RegisterWithManager() {
        if (_sceneContainer == null) {
            LogError("SceneContainer not assigned!");
            return;
        }

        _playerManager = _sceneContainer.GetManager<PlayerManager>();

        if (_playerManager != null) {
            _playerManager.SetPlayerHandler(this);
            Log("Registered with PlayerManager");
        } else {
            LogError("Could not find PlayerManager in SceneContainer!");
        }
    }

    /// <summary>
    /// Initialize all subsystems with their dependencies
    /// Called by PlayerManager after registration
    /// </summary>
    public void Initialize(PlayerDefaultStatsSO defaultStats) {
        if (_isInitialized) {
            LogWarning("Already initialized!");
            return;
        }

        Log("Starting initialization...");

        // Get InputManager from scene container
        _inputManager = _sceneContainer.GetManager<InputManager>();

        if (_inputManager == null) {
            LogError("Could not find InputManager in SceneContainer!");
        }

        // 1. Initialize stats first (other systems depend on it)
        if (defaultStats != null) {
            _statsHandler.LoadFromDefaultStats(defaultStats);
            Log("Stats loaded from defaults");
        } else {
            LogWarning("No default stats provided, using current values");
        }

        // 2. Initialize systems with their dependencies
        _physicsHandler.Initialize(_statsHandler, _blackboardHandler);
        Log("Physics initialized");

        _collisionHandler.Initialize(this, _statsHandler, _blackboardHandler);
        Log("Collision initialized");

        _animationHandler.Initialize(_blackboardHandler);
        Log("Animation initialized");


        if (_inputManager != null) {
            _inputHandler.Initialize(_inputManager, _blackboardHandler, _statsHandler);
            Log("Input initialized");
        } else {
            LogWarning("Skipping input initialization (InputManager not found)");
        }

        // 3. Initialize state machine last (depends on all other systems being ready)
        _stateMachineHandler.Initialize(this);
        Log("State machine initialized");

        _isInitialized = true;
        Log("All systems initialized successfully!");
    }

    #endregion

    #region Update Methods (Called by PlayerManager)

    /// <summary>
    /// Called by PlayerManager during its OnUpdate
    /// Do NOT call this from Unity's Update!
    /// </summary>
    public void OnUpdate() {
        if (!_isInitialized) return;

        // Update timers in blackboard
        _blackboardHandler.UpdateTimers(Time.deltaTime);

        // Update collision checks
        _collisionHandler.UpdateCollisionChecks();

        // Update state machine (which will update current state)
        _stateMachineHandler.OnUpdate();
    }

    /// <summary>
    /// Called by PlayerManager during its OnFixedUpdate
    /// Do NOT call this from Unity's FixedUpdate!
    /// </summary>
    public void OnFixedUpdate() {
        if (!_isInitialized) return;

        // Fixed update state machine (physics-based state logic)
        _stateMachineHandler.OnFixedUpdate();

        // Apply physics (velocity to rigidbody)
        _physicsHandler.ApplyPhysics();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Enable or disable player control
    /// </summary>
    public void SetControlEnabled(bool enabled) {
        _blackboardHandler.CanMove = enabled;
        _blackboardHandler.CanJump = enabled;
    }

    /// <summary>
    /// Teleport player to position
    /// </summary>
    public void Teleport(Vector3 position) {
        transform.position = position;
        _physicsHandler.StopMovement();
    }

    /// <summary>
    /// Apply external force (like knockback)
    /// </summary>
    public void ApplyExternalForce(Vector2 force) {
        _physicsHandler.ApplyDirectionalForce(force);
    }

    /// <summary>
    /// Get current player position
    /// </summary>
    public Vector3 GetPosition() {
        return transform.position;
    }

    /// <summary>
    /// Check if player is initialized
    /// </summary>
    public bool IsInitialized() {
        return _isInitialized;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    private void Log(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.Log($"[PlayerHandler] {i_message}");
        }
    }

    private void LogWarning(string i_message) {
        if (_iEnableDebugLogs) {
            Debug.LogWarning($"[PlayerHandler] {i_message}");
        }
    }

    private void LogError(string i_message) {
        Debug.LogError($"[PlayerHandler] {i_message}");
    }

    #endregion
}