using UnityEngine;

/// <summary>
/// Handles physics calculations and applies forces to the Rigidbody2D
/// Reads from and writes to Blackboard for velocity management
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPhysicsHandler : MonoBehaviour {
    private PlayerStatsHandler _stats;
    private PlayerBlackboardHandler _blackboard;
    private Rigidbody2D _rigidbody2D;

    private void Awake() {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Initialize(PlayerStatsHandler stats, PlayerBlackboardHandler blackboard) {
        _stats = stats;
        _blackboard = blackboard;
        if (_blackboard == null) Debug.Log("Blackboard missing");
    }

    /// <summary>
    /// Called during FixedUpdate to apply velocity to rigidbody
    /// </summary>
    public void ApplyPhysics() {
        if (_blackboard == null) Debug.Log("Blackboard missing");

        // Apply gravity if enabled
        if (!_blackboard.IsGravityDisabled) {
            ApplyGravity();
        } else {
            _blackboard.Velocity.y = 0;
        }

        // Clamp velocities
        ClampVelocity();

        // Apply to rigidbody
        _rigidbody2D.linearVelocity = _blackboard.Velocity;
    }

    /// <summary>
    /// Apply gravity based on current state
    /// States should set the appropriate gravity multiplier
    /// </summary>
    private void ApplyGravity() {
        // States will handle specific gravity values
        // This is just a safety clamp for fall speed
        if (_blackboard.Velocity.y < -_stats.MaxFallSpeed) {
            _blackboard.Velocity.y = -_stats.MaxFallSpeed;
        }
    }

    /// <summary>
    /// Clamp horizontal and vertical velocities
    /// </summary>
    private void ClampVelocity() {
        // Clamp vertical velocity
        _blackboard.Velocity.y = Mathf.Clamp(
            _blackboard.Velocity.y,
            -_stats.MaxFallSpeed,
            100f // Max upward velocity (can be made configurable)
        );

        // Horizontal clamping can be handled by states for more control
    }

    /// <summary>
    /// Apply horizontal acceleration/deceleration
    /// Called by states for ground/air movement
    /// </summary>
    public void ApplyHorizontalMovement(float targetSpeed, float acceleration, float deceleration) {
        float targetVelocity = targetSpeed * _blackboard.MoveInput.x;

        if (Mathf.Abs(_blackboard.MoveInput.x) >= _stats.MoveThreshold) {
            // Accelerate toward target
            _blackboard.Velocity.x = Mathf.Lerp(
                _blackboard.Velocity.x,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
        } else {
            // Decelerate to zero
            _blackboard.Velocity.x = Mathf.Lerp(
                _blackboard.Velocity.x,
                0f,
                deceleration * Time.fixedDeltaTime
            );
        }
    }

    /// <summary>
    /// Apply vertical force (for jumps)
    /// </summary>
    public void ApplyVerticalForce(float force) {
        _blackboard.Velocity.y = force;
    }

    /// <summary>
    /// Add gravity to current velocity
    /// </summary>
    public void ApplyGravityForce(float gravity) {
        _blackboard.Velocity.y += gravity * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Apply wall sliding physics
    /// </summary>
    public void ApplyWallSlide(float targetSpeed, float deceleration) {
        _blackboard.Velocity.y = Mathf.Lerp(
            _blackboard.Velocity.y,
            -targetSpeed,
            deceleration * Time.fixedDeltaTime
        );
    }

    /// <summary>
    /// Apply a directional force (like wall jump)
    /// </summary>
    public void ApplyDirectionalForce(Vector2 force) {
        _blackboard.Velocity = force;
    }

    /// <summary>
    /// Immediately set velocity (use sparingly)
    /// </summary>
    public void SetVelocity(Vector2 velocity) {
        _blackboard.Velocity = velocity;
    }

    /// <summary>
    /// Stop all movement
    /// </summary>
    public void StopMovement() {
        _blackboard.Velocity = Vector2.zero;
        _rigidbody2D.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Stop horizontal movement only
    /// </summary>
    public void StopHorizontalMovement() {
        _blackboard.Velocity.x = 0;
    }

    /// <summary>
    /// Stop vertical movement only
    /// </summary>
    public void StopVerticalMovement() {
        _blackboard.Velocity.y = 0;
    }
}