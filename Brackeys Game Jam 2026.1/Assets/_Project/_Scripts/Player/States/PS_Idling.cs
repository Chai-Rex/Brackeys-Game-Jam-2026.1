using UnityEngine;

/// <summary>
/// Grounded substate: Player is standing still
/// </summary>
public class PS_Idling : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_Idling(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_Idling] Entered");
        }

        // Play idle animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Idle, false);

        // Reset jumping flags
        _stateMachine.Blackboard.IsJumping = false;
        _stateMachine.Blackboard.IsWallJumping = false;
    }

    public override void InitializeSubState() {
        // Idling has no substates
    }

    public override void Update() {
        // Handle idle-specific update logic
    }

    public override void FixedUpdate() {
        // Apply ground physics with deceleration
        _stateMachine.Physics.ApplyHorizontalMovement(
            _stateMachine.Stats.GroundTargetSpeed,
            _stateMachine.Stats.GroundAcceleration,
            _stateMachine.Stats.GroundDeceleration
        );

        // Update facing direction based on input
        _stateMachine.CheckForTurning(_stateMachine.Blackboard.MoveInput);
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check for jump input (using reusable check)
        if (_stateMachine.CheckJump()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
            return;
        }

        // Check for movement input (using reusable check)
        if (_stateMachine.CheckForInputMovement()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Moving));
            return;
        }

        // Check for dodge input
        if (_stateMachine.Blackboard.IsCrouchPressed &&
            _stateMachine.CheckForInputMovement()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Dodging));
            return;
        }
    }

    public override void ExitState() {

    }
}