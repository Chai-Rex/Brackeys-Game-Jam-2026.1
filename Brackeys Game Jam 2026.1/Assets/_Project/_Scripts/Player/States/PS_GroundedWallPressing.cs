using UnityEngine;

/// <summary>
/// Grounded substate: Player is pushing against a wall while on the ground
/// Can transition to wall climb or wall jump if implemented
/// </summary>
public class PS_GroundedWallPressing : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_GroundedWallPressing(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_GroundedWallPressing] Entered");
        }

        // TODO: Play wall press animation
        // For now, use idle animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Idle, false);
    }

    public override void InitializeSubState() {
        // Wall pressing has no substates
    }

    public override void Update() {
        // Handle wall pressing logic
    }

    public override void FixedUpdate() {
        // Apply deceleration to slow down horizontal movement
        _stateMachine.Physics.ApplyHorizontalMovement(
            0f, // Target speed is 0 when pressing against wall
            _stateMachine.Stats.GroundAcceleration,
            _stateMachine.Stats.GroundDeceleration * 2f // Faster deceleration against wall
        );
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check for jump input (could do wall jump from ground)
        if (_stateMachine.Blackboard.JumpBufferTimer > 0 && _stateMachine.Blackboard.CanJump) {
            // Could transition to wall jump here if desired
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
            return;
        }

        // Check if no longer pressing against wall
        if (!_stateMachine.Blackboard.IsAgainstWall) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Moving));
            return;
        }

        // Check if player stopped inputting toward wall
        int inputDirection = _stateMachine.Blackboard.MoveInput.x > 0 ? 1 : -1;
        if (inputDirection != _stateMachine.Blackboard.WallDirection) {
            // Player moved away from wall
            if (Mathf.Abs(_stateMachine.Blackboard.MoveInput.x) > _stateMachine.Stats.MoveThreshold) {
                SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Moving));
            } else {
                SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Idling));
            }
            return;
        }
    }

    public override void ExitState() {

    }
}
