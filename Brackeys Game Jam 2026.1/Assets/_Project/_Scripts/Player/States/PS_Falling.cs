using UnityEngine;

/// <summary>
/// Airborne substate: Player is falling (not jumping)
/// </summary>
public class PS_Falling : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_Falling(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_Falling] Entered");
        }

        // Play falling animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Falling, false);
    }

    public override void InitializeSubState() {
        // Falling has no substates
    }

    public override void Update() {
        // No frame-based logic needed
    }

    public override void FixedUpdate() {
        // Apply gravity
        _stateMachine.Physics.ApplyGravityForce(_stateMachine.Stats.GroundJumpGravity);

        // Apply air control
        _stateMachine.Physics.ApplyHorizontalMovement(
            _stateMachine.Stats.AirborneTargetSpeed,
            _stateMachine.Stats.AirborneAcceleration,
            _stateMachine.Stats.AirborneDeceleration
        );

        // Update facing direction
        _stateMachine.CheckForTurning(_stateMachine.Blackboard.MoveInput);
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check for coyote ground jump (using reusable check)
        if (_stateMachine.CheckCoyoteGroundJump()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.CoyoteGroundJump));
            return;
        }

        // Check for coyote wall jump (using reusable check)
        if (_stateMachine.CheckCoyoteWallJump()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.CoyoteWallJump));
            return;
        }

        // Note: Wall touch and ground touch are handled by parent Airborne state
    }

    public override void ExitState() {

    }
}