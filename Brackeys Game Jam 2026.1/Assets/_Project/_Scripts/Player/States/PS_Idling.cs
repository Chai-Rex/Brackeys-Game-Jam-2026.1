using UnityEngine;

/// <summary>
/// Grounded substate: Player is standing still.
/// </summary>
public class PS_Idling : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_Idling(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_Idling] Entered");

        _sm.Blackboard.IsJumping     = false;
        _sm.Blackboard.IsWallJumping = false;
        _sm.Animation.Play(PlayerAnimationHandler.Idle, false);
    }

    public override void InitializeSubState() { }

    public override void Update() { }

    public override void FixedUpdate() {
        _sm.Physics.ApplyHorizontalMovement(
            _sm.Stats.GroundTargetSpeed,
            _sm.Stats.GroundAcceleration,
            _sm.Stats.GroundDeceleration);
        _sm.CheckForTurning(_sm.Blackboard.MoveInput);
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        if (_sm.CheckJump()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
            return;
        }

        if (_sm.CheckForInputMovement()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Moving));
            return;
        }

        // Dodge: requires both crouch press and directional input
        if (_sm.Blackboard.IsCrouchPressed && _sm.CheckForInputMovement()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Dodging));
            return;
        }
    }

    public override void ExitState() { }
}
