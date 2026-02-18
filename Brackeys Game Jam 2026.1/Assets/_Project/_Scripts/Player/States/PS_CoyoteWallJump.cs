using UnityEngine;

/// <summary>
/// Airborne substate: Player performs a wall jump with coyote time
/// Allows wall jumping shortly after leaving a wall
/// </summary>
public class PS_CoyoteWallJump : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_CoyoteWallJump(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_CoyoteWallJump] Entered - Coyote Wall Jump!");
        }

        // Consume the jump buffer
        _stateMachine.Input.ConsumeJumpBuffer();

        // Set wall jumping flag
        _stateMachine.Blackboard.IsWallJumping = true;

        // Get the direction of the last wall touched
        int lastWallDirection = _stateMachine.Blackboard.WallDirection;

        // If wall direction is 0, use the direction they were facing when they left the wall
        if (lastWallDirection == 0) {
            // Fallback: use opposite of current facing direction
            lastWallDirection = _stateMachine.Blackboard.IsFacingRight ? 1 : -1;

            if (_stateMachine.Blackboard.debugStates) {
                Debug.LogWarning("[PS_CoyoteWallJump] Wall direction was 0, using fallback");
            }
        }

        // Calculate wall jump velocity
        // Jump AWAY from where the wall was
        float horizontalVelocity = -lastWallDirection * _stateMachine.Stats.WallJumpDirection.x;
        float verticalVelocity = _stateMachine.Stats.InitialWallJumpVelocity;

        // DIRECTLY SET VELOCITY
        _stateMachine.Blackboard.Velocity = new Vector2(horizontalVelocity, verticalVelocity);

        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log($"[PS_CoyoteWallJump] Last wall on {(lastWallDirection > 0 ? "RIGHT" : "LEFT")}, " +
                      $"jumping to {(horizontalVelocity > 0 ? "RIGHT" : "LEFT")} " +
                      $"with velocity ({horizontalVelocity:F2}, {verticalVelocity:F2})");
        }

        // Play wall jump animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Jump, false);

        // Update facing direction (away from where wall was)
        _stateMachine.Blackboard.IsFacingRight = lastWallDirection < 0;

        // Immediately transition to wall jumping state
        var factory = _stateMachine.GetFactory();
        SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.WallJumping));
    }

    public override void InitializeSubState() {
        // No substates - this is a transition state
    }

    public override void Update() {
        // This state transitions immediately
    }

    public override void FixedUpdate() {
        // Jump force already applied in EnterState
    }

    public override void CheckSwitchStates() {
        // State switches immediately in EnterState
    }

    public override void ExitState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_CoyoteWallJump] Exited");
        }
    }
}