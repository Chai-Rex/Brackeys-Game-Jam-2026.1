using UnityEngine;

/// <summary>
/// Grounded substate: Player is moving horizontally on the ground.
/// </summary>
public class PS_Moving : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_Moving(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_Moving] Entered");

        UpdateAnimation();
    }

    public override void InitializeSubState() { }

    public override void Update() { }

    public override void FixedUpdate() {
        _sm.Physics.ApplyHorizontalMovement(
            _sm.Stats.GroundTargetSpeed,
            _sm.Stats.GroundAcceleration,
            _sm.Stats.GroundDeceleration);
        _sm.CheckForTurning(_sm.Blackboard.MoveInput);
        UpdateAnimation();
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        if (_sm.CheckJump()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
            return;
        }

        if (_sm.CheckIdling()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Idling));
            return;
        }

        if (_sm.CheckPressingAgainstWall()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedWallPressing));
            return;
        }
    }

    public override void ExitState() { }

    // --- Private --------------------------------------------------------------

    private void UpdateAnimation() {
        float absVel = Mathf.Abs(_sm.Blackboard.Velocity.x);

        if (absVel < _sm.Stats.GroundTargetSpeed / 2f)
            _sm.Animation.Play(PlayerAnimationHandler.Walking, false);
        else if (absVel > _sm.Stats.TurnThreshold)
            _sm.Animation.Play(PlayerAnimationHandler.Sprinting, false);
        else
            _sm.Animation.Play(PlayerAnimationHandler.Running, false);
    }
}
