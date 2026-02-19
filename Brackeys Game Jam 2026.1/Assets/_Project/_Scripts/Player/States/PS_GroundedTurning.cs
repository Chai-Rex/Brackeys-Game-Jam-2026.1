using UnityEngine;

/// <summary>
/// Grounded substate: Player is turning around at speed.
/// Applies stronger deceleration until velocity drops below TurnBuffer.
/// </summary>
public class PS_GroundedTurning : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_GroundedTurning(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_GroundedTurning] Entered");

        _sm.Animation.Play(PlayerAnimationHandler.Turning, false);
    }

    public override void InitializeSubState() { }

    public override void Update() {
        // Flip facing direction immediately so visuals snap to input
        if      (_sm.Blackboard.MoveInput.x > 0) _sm.Blackboard.IsFacingRight = true;
        else if (_sm.Blackboard.MoveInput.x < 0) _sm.Blackboard.IsFacingRight = false;
    }

    public override void FixedUpdate() {
        _sm.Physics.ApplyHorizontalMovement(
            _sm.Stats.GroundTargetSpeed,
            _sm.Stats.GroundAcceleration,
            _sm.Stats.TurnDeceleration);
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        if (_sm.CheckJump()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
            return;
        }

        if (Mathf.Abs(_sm.Blackboard.MoveInput.x) < _sm.Stats.MoveThreshold) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Idling));
            return;
        }

        if (Mathf.Abs(_sm.Blackboard.Velocity.x) < _sm.Stats.TurnBuffer) {
            var next = _sm.CheckForInputMovement()
                ? PlayerStateFactory.PlayerStates.Moving
                : PlayerStateFactory.PlayerStates.Idling;
            SwitchState(factory.GetState(next));
            return;
        }
    }

    public override void ExitState() { }
}
