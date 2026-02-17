using UnityEngine;

/// <summary>
/// Manages the player's hierarchical state machine
/// Provides easy access to all player subsystems for states
/// Contains reusable check methods for common state transitions
/// </summary>
public class PlayerStateMachineHandler : MonoBehaviour, IStateMachineContext {
    [Header("Debug")]
    [SerializeField] private bool _debugStates = false;

    private PlayerHandler _playerHandler;
    private PlayerStateFactory _factory;
    private BaseHierarchicalState _currentState;

    public string _ManagerName => GetType().Name;

    #region Public Accessors for States

    // States can access all player subsystems through the state machine
    public SceneContainerSO SceneContainer => _playerHandler.SceneContainer;
    public PlayerPhysicsHandler Physics => _playerHandler.Physics;
    public PlayerAnimamationHandler Animation => _playerHandler.Animation;
    public PlayerCollisionHandler Collision => _playerHandler.Collision;
    public PlayerStatsHandler Stats => _playerHandler.Stats;
    public PlayerBlackboardHandler Blackboard => _playerHandler.Blackboard;
    public PlayerInputHandler Input => _playerHandler.Input;
    public PlayerHandler Handler => _playerHandler;

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize the state machine
    /// </summary>
    public void Initialize(PlayerHandler playerHandler) {
        _playerHandler = playerHandler;

        if (_playerHandler == null) {
            Debug.LogError($"[{_ManagerName}] PlayerHandler is null!");
            return;
        }

        // Create factory and initialize all states
        _factory = new PlayerStateFactory(this);
        _factory.InitializeStates();

        // Set initial state
        _factory.SetState(PlayerStateFactory.PlayerStates.Grounded);

        if (_debugStates) {
            Debug.Log($"[{_ManagerName}] State machine initialized with state: {_currentState.GetType().Name}");
        }
    }

    #endregion

    #region State Management

    /// <summary>
    /// Set the current state (called by factory or states)
    /// </summary>
    public void SetState(BaseHierarchicalState state) {
        if (_debugStates && _currentState != null) {
            Debug.Log($"[{_ManagerName}] State Change: {_currentState.GetType().Name} -> {state.GetType().Name}");
        }

        _currentState = state;
    }

    /// <summary>
    /// Get the current state
    /// </summary>
    public BaseHierarchicalState GetCurrentState() {
        return _currentState;
    }

    /// <summary>
    /// Get the state factory
    /// </summary>
    public PlayerStateFactory GetFactory() {
        return _factory;
    }

    /// <summary>
    /// Update the state machine (called by PlayerHandler in Update)
    /// </summary>
    public void OnUpdate() {
        _currentState?.UpdateStates();
    }

    /// <summary>
    /// Fixed update the state machine (called by PlayerHandler in FixedUpdate)
    /// </summary>
    public void OnFixedUpdate() {
        _currentState?.FixedUpdateStates();
    }

    /// <summary>
    /// Force a state change (use sparingly, prefer letting states handle transitions)
    /// </summary>
    public void ForceStateChange(PlayerStateFactory.PlayerStates state) {
        _factory.SetState(state);
    }

    #endregion

    #region Reusable State Check Methods

    // ==================== JUMP CHECKS ====================

    /// <summary>
    /// Check if player can jump (has input and ability)
    /// </summary>
    public bool CheckJump() {
        return Blackboard.JumpBufferTimer > 0 && Blackboard.CanJump;
    }

    /// <summary>
    /// Check if jump button is being held (for variable jump height)
    /// </summary>
    public bool CheckJumpSustained() {
        return Blackboard.IsJumpSustained;
    }

    /// <summary>
    /// Check if player hit their head (ceiling collision)
    /// </summary>
    public bool CheckHeadHit() {
        return Blackboard.IsHeadBlocked;
    }

    // ==================== MOVEMENT CHECKS ====================

    /// <summary>
    /// Check if player is providing movement input
    /// </summary>
    public bool CheckForInputMovement() {
        return Mathf.Abs(Blackboard.MoveInput.x) > Stats.MoveThreshold;
    }

    /// <summary>
    /// Check if player should be idling (no meaningful input)
    /// </summary>
    public bool CheckIdling() {
        return Mathf.Abs(Blackboard.MoveInput.x) < Stats.MoveThreshold &&
               Mathf.Abs(Blackboard.Velocity.x) < Stats.MoveThreshold;
    }

    /// <summary>
    /// Check if player is trying to turn around (moving opposite to velocity at high speed)
    /// </summary>
    public bool CheckMovementTurn() {
        // Not moving fast enough to need turn state
        if (Mathf.Abs(Blackboard.Velocity.x) < Stats.TurnThreshold)
            return false;

        // Check if input is opposite to current velocity
        float velocityDot = Blackboard.Velocity.x * Blackboard.MoveInput.x;
        return velocityDot < 0 && Mathf.Abs(Blackboard.MoveInput.x) > Stats.MoveThreshold;
    }

    /// <summary>
    /// Update facing direction based on movement input
    /// </summary>
    public void CheckForTurning(Vector2 moveInput) {
        if (moveInput.x > Stats.MoveThreshold) {
            Blackboard.IsFacingRight = true;
        } else if (moveInput.x < -Stats.MoveThreshold) {
            Blackboard.IsFacingRight = false;
        }
    }

    // ==================== AIR STATE CHECKS ====================

    /// <summary>
    /// Check if player is falling (negative vertical velocity)
    /// </summary>
    public bool CheckFalling() {
        return Blackboard.Velocity.y < -0.01f;
    }

    /// <summary>
    /// Check if player is at jump apex (for hang time)
    /// Uses InverseLerp to get percentage from max velocity to 0
    /// </summary>
    public bool CheckAtApex(float initialJumpVelocity) {
        float apexPoint = Mathf.InverseLerp(initialJumpVelocity, 0f, Blackboard.Velocity.y);
        return apexPoint > Stats.ApexThreshold;
    }

    // ==================== COYOTE TIME CHECKS ====================

    /// <summary>
    /// Check if player can perform coyote ground jump
    /// </summary>
    public bool CheckCoyoteGroundJump() {
        return Blackboard.CoyoteTimer > 0 &&
               Blackboard.JumpBufferTimer > 0 &&
               Blackboard.CanJump;
    }

    /// <summary>
    /// Check if player can perform coyote wall jump
    /// </summary>
    public bool CheckCoyoteWallJump() {
        return Blackboard.LastWallTime > 0 &&
               (Time.time - Blackboard.LastWallTime) < Stats.CoyoteTime &&
               Blackboard.JumpBufferTimer > 0 &&
               Blackboard.CanJump;
    }

    // ==================== WALL CHECKS ====================

    /// <summary>
    /// Check if wall was on the right side
    /// </summary>
    public bool IsLastWallRight() {
        return Blackboard.WallDirection > 0;
    }

    /// <summary>
    /// Check if wall was on the left side
    /// </summary>
    public bool IsLastWallLeft() {
        return Blackboard.WallDirection < 0;
    }

    /// <summary>
    /// Check if player is against wall and moving toward it
    /// </summary>
    public bool CheckPressingAgainstWall() {
        if (!Blackboard.IsAgainstWall) return false;

        int inputDirection = Blackboard.MoveInput.x > 0 ? 1 : -1;
        return inputDirection == Blackboard.WallDirection;
    }

    #endregion

    #region Debug Helpers

    /// <summary>
    /// Get the name of the current state for debugging
    /// </summary>
    public string GetCurrentStateName() {
        return _currentState?.GetType().Name ?? "None";
    }

    /// <summary>
    /// Get the current state hierarchy as a string
    /// </summary>
    public string GetStateHierarchy() {
        if (_currentState == null) return "None";

        string hierarchy = _currentState.GetType().Name;
        BaseHierarchicalState subState = _currentState.GetCurrentSubState();

        while (subState != null) {
            hierarchy += $" -> {subState.GetType().Name}";
            subState = subState.GetCurrentSubState();
        }

        return hierarchy;
    }

    #endregion
}