using UnityEngine;

/// <summary>
/// Airborne substate: Player is in the ascending phase of a ground jump
/// Handles variable jump height (release button early for lower jump)
/// </summary>
public class PS_GroundJumping : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;
    private float _jumpTimer;
    private bool _isCancelable;

    public PS_GroundJumping(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_GroundJumping] Entered");
        }

        _jumpTimer = 0f;
        _isCancelable = false;

        // Play jumping animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Jumping, false);
    }

    public override void InitializeSubState() {
        // GroundJumping has no substates
    }

    public override void Update() {
        _jumpTimer += Time.deltaTime;

        // Enable canceling after minimum jump time
        if (!_isCancelable && _jumpTimer >= _stateMachine.Stats.GroundJumpCancelableTime) {
            _isCancelable = true;
        }
    }

    public override void FixedUpdate() {
        // Apply jump gravity during ascent
        float gravityToApply = _stateMachine.Stats.GroundJumpGravity;

        // VARIABLE JUMP HEIGHT: Check if player released jump button early
        if (!_stateMachine.CheckJumpSustained() && _isCancelable) {
            // Apply stronger gravity multiplier for shorter jump
            gravityToApply *= _stateMachine.Stats.GroundJumpReleaseGravityMultiplier;

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

        // Update facing direction
        _stateMachine.CheckForTurning(_stateMachine.Blackboard.MoveInput);

        // Apply gravity
        _stateMachine.Physics.ApplyGravityForce(gravityToApply);
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check if hit head on ceiling
        if (_stateMachine.CheckHeadHit()) {
            _stateMachine.Blackboard.Velocity.y = 0;
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }

        // Check if we've reached apex for hang time
        if (_stateMachine.CheckAtApex(_stateMachine.Stats.InitialGroundJumpVelocity)) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.AirHanging));
            return;
        }

        // Check if started falling (shouldn't happen before apex, but safety check)
        if (_stateMachine.Blackboard.Velocity.y <= 0) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }
    }

    public override void ExitState() {
        _stateMachine.Blackboard.IsJumping = false;
    }
}