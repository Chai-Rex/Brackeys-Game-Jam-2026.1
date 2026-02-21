using UnityEngine;

/// <summary>
/// Manages the player's hierarchical state machine.
/// Provides access to all player subsystems and exposes reusable transition checks
/// so individual states stay lean and don't duplicate logic.
/// </summary>
public class PlayerStateMachineHandler : MonoBehaviour, IStateMachineContext {
    [Header("Debug")]
    [SerializeField] private bool _debugStates = false;

    private PlayerHandler _playerHandler;
    private PlayerStateFactory _factory;
    private BaseHierarchicalState _currentState;

    public string _ManagerName => GetType().Name;

    // --- Subsystem Accessors --------------------------------------------------

    public SceneContainerSO SceneContainer => _playerHandler.SceneContainer;
    public PlayerPhysicsHandler Physics     => _playerHandler.Physics;
    public PlayerAnimationHandler Animation => _playerHandler.Animation;
    public PlayerCollisionHandler Collision => _playerHandler.Collision;
    public PlayerStatsHandler Stats         => _playerHandler.Stats;
    public PlayerBlackboardHandler Blackboard => _playerHandler.Blackboard;
    public PlayerInputHandler Input         => _playerHandler.Input;
    public PlayerHandler Handler            => _playerHandler;

    // --- Initialization -------------------------------------------------------

    public void Initialize(PlayerHandler playerHandler) {
        _playerHandler = playerHandler;

        if (_playerHandler == null) {
            Debug.LogError($"[{_ManagerName}] PlayerHandler is null!");
            return;
        }

        _factory = new PlayerStateFactory(this);
        _factory.InitializeStates();
        _factory.SetState(PlayerStateFactory.PlayerStates.Airborne);

        if (_debugStates) {
            Debug.Log($"[{_ManagerName}] Initialized -> {_currentState?.GetType().Name}");
        }
    }

    // --- IStateMachineContext -------------------------------------------------

    public void SetState(BaseHierarchicalState state) {
        if (_debugStates && _currentState != null) {
            Debug.Log($"[{_ManagerName}] {_currentState.GetType().Name} -> {state.GetType().Name}");
        }

        _currentState = state;
    }

    // --- State Machine Tick ---------------------------------------------------

    public void OnUpdate()      => _currentState?.UpdateStates();
    public void OnFixedUpdate() => _currentState?.FixedUpdateStates();

    // --- Factory / State Access -----------------------------------------------

    public PlayerStateFactory GetFactory()             => _factory;
    public BaseHierarchicalState GetCurrentState()     => _currentState;
    public void ForceStateChange(PlayerStateFactory.PlayerStates state) => _factory.SetState(state);

    // --- Jump Checks ----------------------------------------------------------

    /// <summary>Returns true if the player has a buffered jump and is able to jump.</summary>
    public bool CheckJump() =>
        Blackboard.JumpBufferTimer > 0 && Blackboard.CanJump;

    /// <summary>Returns true while the jump button is held down (variable jump height).</summary>
    public bool CheckJumpSustained() => Blackboard.IsJumpSustained;

    /// <summary>Returns true if the player's head is currently blocked by a ceiling.</summary>
    public bool CheckHeadHit() => Blackboard.IsHeadBlocked;

    // --- Movement Checks ------------------------------------------------------

    /// <summary>Returns true if horizontal move input exceeds the move threshold.</summary>
    public bool CheckForInputMovement() =>
        Mathf.Abs(Blackboard.MoveInput.x) > Stats.MoveThreshold;

    /// <summary>Returns true when input and velocity are both below threshold (player has stopped).</summary>
    public bool CheckIdling() =>
        Mathf.Abs(Blackboard.MoveInput.x) < Stats.MoveThreshold &&
        Mathf.Abs(Blackboard.Velocity.x)  < Stats.MoveThreshold;

    /// <summary>Returns true if the player is inputting opposite to their current velocity at high speed.</summary>
    public bool CheckMovementTurn() {
        if (Mathf.Abs(Blackboard.Velocity.x) < Stats.TurnThreshold) return false;
        float dot = Blackboard.Velocity.x * Blackboard.MoveInput.x;
        return dot < 0 && Mathf.Abs(Blackboard.MoveInput.x) > Stats.MoveThreshold;
    }

    /// <summary>Updates IsFacingRight from horizontal input.</summary>
    public void CheckForTurning(Vector2 moveInput) {
        if      (moveInput.x >  Stats.MoveThreshold) Blackboard.IsFacingRight = true;
        else if (moveInput.x < -Stats.MoveThreshold) Blackboard.IsFacingRight = false;
    }

    // --- Air State Checks -----------------------------------------------------

    /// <summary>Returns true when vertical velocity is meaningfully negative.</summary>
    public bool CheckFalling() => Blackboard.Velocity.y < -0.01f;

    /// <summary>
    /// Returns true when the player is near the apex of a jump.
    /// Uses InverseLerp from initialJumpVelocity -> 0 and compares against ApexThreshold.
    /// </summary>
    public bool CheckAtApex(float initialJumpVelocity) {
        float apexPoint = Mathf.InverseLerp(initialJumpVelocity, 0f, Blackboard.Velocity.y);
        return apexPoint > Stats.ApexThreshold;
    }

    // --- Coyote Time Checks ---------------------------------------------------

    /// <summary>Returns true if a coyote ground jump is valid this frame.</summary>
    public bool CheckCoyoteGroundJump() =>
        Blackboard.CoyoteTimer > 0 &&
        Blackboard.JumpBufferTimer > 0 &&
        Blackboard.CanJump;

    /// <summary>Returns true if a coyote wall jump is valid this frame.</summary>
    public bool CheckCoyoteWallJump() =>
        Blackboard.LastWallTime > 0 &&
        (Time.time - Blackboard.LastWallTime) < Stats.CoyoteTime &&
        Blackboard.JumpBufferTimer > 0 &&
        Blackboard.CanJump;

    // --- Wall Checks ----------------------------------------------------------

    /// <summary>Returns true if the player is against a wall and pushing toward it.</summary>
    public bool CheckPressingAgainstWall() {
        if (!Blackboard.IsAgainstWall) return false;
        int inputDir = Blackboard.MoveInput.x > 0 ? 1 : -1;
        return inputDir == Blackboard.WallDirection;
    }

    public bool IsLastWallRight() => Blackboard.WallDirection > 0;
    public bool IsLastWallLeft()  => Blackboard.WallDirection < 0;

    // --- Debug Helpers --------------------------------------------------------

    public string GetCurrentStateName() => _currentState?.GetType().Name ?? "None";

    public string GetStateHierarchy() {
        if (_currentState == null) return "None";

        var sb = new System.Text.StringBuilder(_currentState.GetType().Name);
        BaseHierarchicalState sub = _currentState.GetCurrentSubState();

        while (sub != null) {
            sb.Append(" -> ").Append(sub.GetType().Name);
            sub = sub.GetCurrentSubState();
        }

        return sb.ToString();
    }
}
