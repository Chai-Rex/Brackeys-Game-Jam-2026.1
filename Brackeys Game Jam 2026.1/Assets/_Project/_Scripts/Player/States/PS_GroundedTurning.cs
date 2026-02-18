using UnityEngine;

/// <summary>
/// Grounded substate: Player is turning around while moving at speed
/// Handles the deceleration when changing direction
/// </summary>
public class PS_GroundedTurning : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_GroundedTurning(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_GroundedTurning] Entered");
        }

        // Play turning animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Turning, false);
    }

    public override void InitializeSubState() {
        // Turning has no substates
    }

    public override void Update() {
        // Update facing direction immediately
        if (_stateMachine.Blackboard.MoveInput.x > 0) {
            _stateMachine.Blackboard.IsFacingRight = true;
        } else if (_stateMachine.Blackboard.MoveInput.x < 0) {
            _stateMachine.Blackboard.IsFacingRight = false;
        }
    }

    public override void FixedUpdate() {
        // Apply stronger deceleration when turning
        _stateMachine.Physics.ApplyHorizontalMovement(
            _stateMachine.Stats.GroundTargetSpeed,
            _stateMachine.Stats.GroundAcceleration,
            _stateMachine.Stats.TurnDeceleration
        );
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check for jump input
        if (_stateMachine.Blackboard.JumpBufferTimer > 0 && _stateMachine.Blackboard.CanJump) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
            return;
        }

        // Check if velocity is low enough to exit turn state
        if (Mathf.Abs(_stateMachine.Blackboard.Velocity.x) < _stateMachine.Stats.TurnBuffer) {
            // Transition to moving in the new direction
            if (Mathf.Abs(_stateMachine.Blackboard.MoveInput.x) > _stateMachine.Stats.MoveThreshold) {
                SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Moving));
            } else {
                SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Idling));
            }
            return;
        }

        // If player stopped inputting movement, go to idle
        if (Mathf.Abs(_stateMachine.Blackboard.MoveInput.x) < _stateMachine.Stats.MoveThreshold) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Idling));
            return;
        }
    }

    public override void ExitState() {

    }
}
