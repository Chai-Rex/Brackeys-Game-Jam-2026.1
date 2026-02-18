using UnityEngine;

/// <summary>
/// OnWall substate: Player initiates a wall jump from the wall
/// This is the "wall jump start" state that applies force AWAY FROM WALL and transitions to airborne
/// </summary>
public class PS_WallJump : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_WallJump(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_WallJump] Entered - Executing Wall Jump");
        }

        // Consume the jump buffer
        _stateMachine.Input.ConsumeJumpBuffer();

        // Set wall jumping flag
        _stateMachine.Blackboard.IsWallJumping = true;

        // Get wall direction (which side the wall is on)
        int wallDirection = _stateMachine.Blackboard.WallDirection;

        if (wallDirection == 0) {
            Debug.LogWarning("[PS_WallJump] Wall direction is 0! Falling back to facing direction.");
            wallDirection = _stateMachine.Blackboard.IsFacingRight ? 1 : -1;
        }

        // Calculate wall jump velocity
        // If wall is on RIGHT (wallDirection = 1), jump LEFT (negative X)
        // If wall is on LEFT (wallDirection = -1), jump RIGHT (positive X)
        float horizontalVelocity = -wallDirection * _stateMachine.Stats.WallJumpDirection.x;
        float verticalVelocity = _stateMachine.Stats.InitialWallJumpVelocity;

        // DIRECTLY SET VELOCITY - This is the key!
        _stateMachine.Blackboard.Velocity = new Vector2(horizontalVelocity, verticalVelocity);

        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log($"[PS_WallJump] Wall on {(wallDirection > 0 ? "RIGHT" : "LEFT")}, " +
                      $"jumping to {(horizontalVelocity > 0 ? "RIGHT" : "LEFT")} " +
                      $"with velocity ({horizontalVelocity:F2}, {verticalVelocity:F2})");
        }

        // Play wall jump animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Jump, false);

        // Update facing direction (player faces AWAY from wall after jump)
        _stateMachine.Blackboard.IsFacingRight = wallDirection < 0;
    }

    public override void InitializeSubState() {
        // No substates - this is a transition state
    }

    public override void Update() {
        // This state is only active for one frame
    }

    public override void FixedUpdate() {
        // Jump force already applied in EnterState
    }

    public override void CheckSwitchStates() {
        // State switches immediately in EnterState
    }

    public override void ExitState() {

    }
}