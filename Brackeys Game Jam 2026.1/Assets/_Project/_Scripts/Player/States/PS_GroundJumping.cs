using UnityEngine;

/// <summary>
/// Airborne substate: Ascending phase of a ground jump.
/// Handles variable jump height and transitions to AirHanging at apex.
/// </summary>
public class PS_GroundJumping : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;
    private float _jumpTimer;
    private bool _isCancelable;

    public PS_GroundJumping(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_GroundJumping] Entered");

        _jumpTimer   = 0f;
        _isCancelable = false;

        _sm.Animation.Play(PlayerAnimationHandler.Jumping, false);
    }

    public override void InitializeSubState() { }

    public override void Update() {
        _jumpTimer += Time.deltaTime;

        if (!_isCancelable && _jumpTimer >= _sm.Stats.GroundJumpCancelableTime)
            _isCancelable = true;
    }

    public override void FixedUpdate() {
        float gravity = _sm.Stats.GroundJumpGravity;
        bool jumpReleased = !_sm.CheckJumpSustained() && _isCancelable;

        if (jumpReleased) {
            gravity *= _sm.Stats.GroundJumpReleaseGravityMultiplier;
            _sm.Physics.ApplyHorizontalMovement(
                _sm.Stats.ApexHangTargetSpeed,
                _sm.Stats.ApexHangAcceleration,
                _sm.Stats.ApexHangDeceleration);
        } else {
            _sm.Physics.ApplyHorizontalMovement(
                _sm.Stats.AirborneTargetSpeed,
                _sm.Stats.AirborneAcceleration,
                _sm.Stats.AirborneDeceleration);
        }

        _sm.CheckForTurning(_sm.Blackboard.MoveInput);
        _sm.Physics.ApplyGravityForce(gravity);
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        // Head collision — kill vertical momentum and fall
        if (_sm.CheckHeadHit()) {
            _sm.Blackboard.Velocity.y = 0;
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }

        // Reached apex — enter hang time
        if (_isCancelable && _sm.CheckAtApex(_sm.Stats.InitialGroundJumpVelocity)) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.AirHanging));
            return;
        }

        // Safety: already falling (e.g. jump canceled very early)
        if (_sm.Blackboard.Velocity.y <= 0) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }
    }

    public override void ExitState() {
        _sm.Blackboard.IsJumping = false;
    }
}
