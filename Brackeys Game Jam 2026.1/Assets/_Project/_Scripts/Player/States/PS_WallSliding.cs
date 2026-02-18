using UnityEngine;

/// <summary>
/// OnWall substate: Player is sliding down a wall
/// </summary>
public class PS_WallSliding : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_WallSliding(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_WallSliding] Entered");
        }

        // Play wall sliding animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.WallSliding, false);

        // Update facing direction (face the wall)
        _stateMachine.Blackboard.IsFacingRight = _stateMachine.Blackboard.WallDirection > 0;
    }

    public override void InitializeSubState() {
        // Wall sliding has no substates
    }

    public override void Update() {
        // Handle wall sliding logic
    }

    public override void FixedUpdate() {

        // Only apply wall slide physics if player is moving downwards (prevents sticking to wall when moving upwards)
        if (_stateMachine.Blackboard.Velocity.y <= 0) {
            // Apply wall slide physics (slower fall than normal)
            _stateMachine.Physics.ApplyWallSlide(
                _stateMachine.Stats.WallSlideTargetSpeed,
                _stateMachine.Stats.WallSlideDeceleration
            );
        }

        // Apply ground movement (Move off wall)
        _stateMachine.Physics.ApplyHorizontalMovement(
            _stateMachine.Stats.GroundTargetSpeed,
            _stateMachine.Stats.GroundAcceleration,
            _stateMachine.Stats.GroundDeceleration
        );
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check for wall jump input
        if (_stateMachine.Blackboard.JumpBufferTimer > 0 && _stateMachine.Blackboard.CanJump) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.WallJump));
            return;
        }

        // Check if player is moving away from wall
        int inputDirection = _stateMachine.Blackboard.MoveInput.x > 0 ? 1 : -1;
        if (Mathf.Abs(_stateMachine.Blackboard.MoveInput.x) > _stateMachine.Stats.MoveThreshold) {
            if (inputDirection != _stateMachine.Blackboard.WallDirection) {
                // Player pushing away from wall - fall off
                // The parent OnWall state will handle transition to Airborne
            }
        }
    }

    public override void ExitState() {

    }
}
