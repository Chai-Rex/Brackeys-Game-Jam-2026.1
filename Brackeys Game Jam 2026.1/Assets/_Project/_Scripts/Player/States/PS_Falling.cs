using UnityEngine;

/// <summary>
/// Airborne substate: Player is falling (downward velocity, no active jump).
/// Handles coyote time jumps and air control while descending.
/// </summary>
public class PS_Falling : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_Falling(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_Falling] Entered");

        _sm.Animation.Play(PlayerAnimationHandler.Falling, false);
    }

    public override void InitializeSubState() { }

    public override void Update() { }

    public override void FixedUpdate() {
        _sm.Physics.ApplyGravityForce(_sm.Stats.GroundJumpGravity);
        _sm.Physics.ApplyHorizontalMovement(
            _sm.Stats.AirborneTargetSpeed,
            _sm.Stats.AirborneAcceleration,
            _sm.Stats.AirborneDeceleration);
        _sm.CheckForTurning(_sm.Blackboard.MoveInput);
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        if (_sm.CheckCoyoteGroundJump()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.CoyoteGroundJump));
            return;
        }

        if (_sm.CheckCoyoteWallJump()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.CoyoteWallJump));
            return;
        }
    }

    public override void ExitState() { }
}
