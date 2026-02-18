using UnityEngine;

/// <summary>
/// Airborne substate: Player is in the ascending phase of a wall jump
/// Handles variable jump height and limited directional control during initial phase
/// </summary>
public class PS_WallJumping : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;
    private float _jumpTimer;
    private bool _isCancelable;
    private int _jumpAwayDirection; // Direction we jumped away from wall

    public PS_WallJumping(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_WallJumping] Entered");
        }

        _jumpTimer = 0f;
        _isCancelable = false;

        // Store which direction we jumped (opposite of wall)
        _jumpAwayDirection = _stateMachine.Blackboard.IsFacingRight ? 1 : -1;

        // Play jumping animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Jumping, false);

        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log($"[PS_WallJumping] Jump away direction: {(_jumpAwayDirection > 0 ? "RIGHT" : "LEFT")}");
        }
    }

    public override void InitializeSubState() {
        // WallJumping has no substates
    }

    public override void Update() {
        _jumpTimer += Time.deltaTime;

        // Enable canceling after minimum jump time
        if (!_isCancelable && _jumpTimer >= _stateMachine.Stats.WallJumpCancelableTime) {
            _isCancelable = true;
        }
    }

    public override void FixedUpdate() {
        // Apply wall jump gravity
        float gravityToApply = _stateMachine.Stats.WallJumpGravity;


        // CONTROL LOCK SYSTEM - Preserve wall jump momentum
        if (_isCancelable) {
            // VARIABLE JUMP HEIGHT: Check if player released jump button early
            if (!_stateMachine.CheckJumpSustained()) {
                gravityToApply *= _stateMachine.Stats.WallJumpReleaseGravityMultiplier;

                // Apply horizontal air control
                _stateMachine.Physics.ApplyHorizontalMovement(
                    _stateMachine.Stats.ApexHangTargetSpeed,
                    _stateMachine.Stats.ApexHangAcceleration,
                    _stateMachine.Stats.ApexHangDeceleration
                );
            } else {
                // Apply horizontal air control
                _stateMachine.Physics.ApplyHorizontalMovement(
                    _stateMachine.Stats.AirborneTargetSpeed,
                    _stateMachine.Stats.AirborneAcceleration,
                    _stateMachine.Stats.AirborneDeceleration
                );
            }
        }


        _stateMachine.Physics.ApplyGravityForce(gravityToApply);

    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check if player released jump early AND is cancelable
        if (!_stateMachine.CheckJumpSustained() && _isCancelable) {
            _stateMachine.Blackboard.Velocity.y = 0;
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }

        // Check if hit head on ceiling
        if (_stateMachine.CheckHeadHit()) {
            _stateMachine.Blackboard.Velocity.y = 0;
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }

        // Check if we've reached apex for hang time
        if (_stateMachine.CheckAtApex(_stateMachine.Stats.InitialWallJumpVelocity)) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.AirHanging));
            return;
        }

        // Check if started falling
        if (_stateMachine.Blackboard.Velocity.y <= 0) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }
    }

    public override void ExitState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_WallJumping] Exited");
        }

        _stateMachine.Blackboard.IsWallJumping = false;
    }
}