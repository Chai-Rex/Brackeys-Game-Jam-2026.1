using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central data store for player state information
/// States read from and write to this blackboard
/// </summary>
public class PlayerBlackboardHandler : MonoBehaviour {
    [Header("Debug")]
    public bool debugStates = false;

    [Header("Physics State")]
    public Vector2 Velocity;
    public bool IsGravityDisabled;

    [Header("Collision State")]
    public bool IsGrounded;
    public bool IsAgainstWall;
    public bool IsHeadBlocked;
    public int WallDirection; // -1 for left, 1 for right, 0 for none

    [Header("Input State")]
    public Vector2 MoveInput;
    public bool IsJumpPressed;
    public bool IsJumpSustained;
    public bool IsCrouchPressed;

    [Header("Timers")]
    public float CoyoteTimer;
    public float JumpBufferTimer;
    public float LastGroundedTime;
    public float LastWallTime;

    [Header("State Flags")]
    public bool CanMove = true;
    public bool CanJump = true;
    public bool IsJumping;
    public bool IsWallJumping;

    [Header("Animation")]
    public int CurrentAnimationHash;

    // Direction tracking
    private bool _isFacingRight = true;

    /// <summary>
    /// Event fired when facing direction changes
    /// Parameters: bool isFacingRight
    /// </summary>
    public UnityAction<bool> OnDirectionChanged;

    /// <summary>
    /// Property for facing direction with change detection
    /// </summary>
    public bool IsFacingRight {
        get => _isFacingRight;
        set {
            if (_isFacingRight != value) {
                _isFacingRight = value;
                OnDirectionChanged?.Invoke(_isFacingRight);
            }
        }
    }

    /// <summary>
    /// Resets timers that should tick down each frame
    /// </summary>
    public void UpdateTimers(float deltaTime) {
        if (CoyoteTimer > 0) CoyoteTimer -= deltaTime;
        if (JumpBufferTimer > 0) JumpBufferTimer -= deltaTime;
    }

    /// <summary>
    /// Called when player lands on ground
    /// </summary>
    public void OnLanded() {
        IsJumping = false;
        IsWallJumping = false;
        LastGroundedTime = Time.time;
        CoyoteTimer = 0; // Reset coyote timer on landing
    }

    /// <summary>
    /// Called when player leaves ground
    /// </summary>
    public void OnLeftGround() {
        // Coyote timer will be set by states if needed
    }

    /// <summary>
    /// Called when player touches wall
    /// </summary>
    public void OnWallTouch() {
        LastWallTime = Time.time;
    }
}