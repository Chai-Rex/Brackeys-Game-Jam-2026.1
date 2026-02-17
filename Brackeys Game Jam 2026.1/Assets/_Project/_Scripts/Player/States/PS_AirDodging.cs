using UnityEngine;

/// <summary>
/// Airborne substate: Player performs an air dodge/dash
/// </summary>
public class PS_AirDodging : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;
    private float _dodgeTimer;
    private const float AIR_DODGE_DURATION = 0.25f;
    private Vector2 _dodgeDirection;

    public PS_AirDodging(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_AirDodging] Entered");
        }

        _dodgeTimer = 0f;

        // Determine dodge direction (can include vertical component)
        Vector2 inputDirection = _stateMachine.Blackboard.MoveInput.normalized;

        if (inputDirection.magnitude < 0.1f) {
            // No input - dodge in facing direction
            inputDirection = _stateMachine.Blackboard.IsFacingRight ? Vector2.right : Vector2.left;
        }

        _dodgeDirection = inputDirection;

        // Apply dodge force
        float dodgeSpeed = _stateMachine.Stats.DodgingDistance / AIR_DODGE_DURATION;
        _stateMachine.Physics.SetVelocity(_dodgeDirection * dodgeSpeed);

        // Disable gravity during dodge
        _stateMachine.Blackboard.IsGravityDisabled = true;

        // TODO: Play air dodge animation
        // _stateMachine.Animation.Play(PlayerAnimamationHandler.AirDodging, true);

        // Temporarily disable certain abilities
        _stateMachine.Blackboard.CanJump = false;
    }

    public override void InitializeSubState() {
        // Air dodging has no substates
    }

    public override void Update() {
        _dodgeTimer += Time.deltaTime;
    }

    public override void FixedUpdate() {
        // Maintain dodge velocity
        float dodgeSpeed = _stateMachine.Stats.DodgingDistance / AIR_DODGE_DURATION;
        _stateMachine.Blackboard.Velocity = _dodgeDirection * dodgeSpeed;
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check if dodge completed
        if (_dodgeTimer >= AIR_DODGE_DURATION) {
            // Re-enable abilities and gravity
            _stateMachine.Blackboard.CanJump = true;
            _stateMachine.Blackboard.IsGravityDisabled = false;

            // Transition to falling
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }
    }

    public override void ExitState() {

        // Ensure gravity and abilities are restored
        _stateMachine.Blackboard.IsGravityDisabled = false;
        _stateMachine.Blackboard.CanJump = true;
    }
}
