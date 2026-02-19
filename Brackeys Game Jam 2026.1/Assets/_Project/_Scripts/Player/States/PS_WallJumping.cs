using UnityEngine;

/// <summary>
/// Airborne substate: Ascending phase of a wall jump.
/// Enforces a brief control-lock window to preserve initial wall jump momentum.
/// </summary>
public class PS_WallJumping : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;
    private float _jumpTimer;
    private bool _isCancelable;

    public PS_WallJumping(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_WallJumping] Entered");

        _jumpTimer    = 0f;
        _isCancelable = false;

        _sm.Animation.Play(PlayerAnimationHandler.Jumping, false);
    }

    public override void InitializeSubState() { }

    public override void Update() {
        _jumpTimer += Time.deltaTime;

        if (!_isCancelable && _jumpTimer >= _sm.Stats.WallJumpCancelableTime)
            _isCancelable = true;
    }

    public override void FixedUpdate() {
        float gravity = _sm.Stats.WallJumpGravity;

        // Only allow air control after the lock-out window has passed
        if (_isCancelable) {
            bool jumpReleased = !_sm.CheckJumpSustained();

            if (jumpReleased) {
                gravity *= _sm.Stats.WallJumpReleaseGravityMultiplier;
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
        }

        _sm.Physics.ApplyGravityForce(gravity);
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        // Variable jump height — cut the jump short on button release
        if (_isCancelable && !_sm.CheckJumpSustained()) {
            _sm.Blackboard.Velocity.y = 0;
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }

        // Head collision
        if (_sm.CheckHeadHit()) {
            _sm.Blackboard.Velocity.y = 0;
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }

        // Apex — enter hang time
        if (_sm.CheckAtApex(_sm.Stats.InitialWallJumpVelocity)) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.AirHanging));
            return;
        }

        // Safety: already falling
        if (_sm.Blackboard.Velocity.y <= 0) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }
    }

    public override void ExitState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_WallJumping] Exited");

        _sm.Blackboard.IsWallJumping = false;
    }
}
