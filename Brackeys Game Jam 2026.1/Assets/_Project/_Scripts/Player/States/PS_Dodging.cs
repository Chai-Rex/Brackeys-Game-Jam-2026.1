using UnityEngine;

/// <summary>
/// Grounded substate: Player performs a dodge/dash on the ground
/// </summary>
public class PS_Dodging : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;
    private float _dodgeTimer;
    private const float DODGE_DURATION = 0.3f;
    private Vector2 _dodgeDirection;

    public PS_Dodging(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_Dodging] Entered");
        }

        _dodgeTimer = 0f;

        // Determine dodge direction based on input
        _dodgeDirection = _stateMachine.Blackboard.MoveInput.x > 0 ? Vector2.right : Vector2.left;

        // Apply dodge force
        float dodgeSpeed = _stateMachine.Stats.DodgingDistance / DODGE_DURATION;
        _stateMachine.Physics.SetVelocity(_dodgeDirection * dodgeSpeed);

        // TODO: Play dodge animation
        // _stateMachine.Animation.Play(PlayerAnimamationHandler.Dodging, true);

        // Temporarily disable certain abilities during dodge
        _stateMachine.Blackboard.CanJump = false;
    }

    public override void InitializeSubState() {
        // Dodging has no substates
    }

    public override void Update() {
        _dodgeTimer += Time.deltaTime;
    }

    public override void FixedUpdate() {
        // Maintain dodge velocity (no deceleration during active dodge)
        float dodgeSpeed = _stateMachine.Stats.DodgingDistance / DODGE_DURATION;
        _stateMachine.Blackboard.Velocity.x = _dodgeDirection.x * dodgeSpeed;
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check if dodge duration completed
        if (_dodgeTimer >= DODGE_DURATION) {
            // Re-enable abilities
            _stateMachine.Blackboard.CanJump = true;

            // Transition based on input
            if (Mathf.Abs(_stateMachine.Blackboard.MoveInput.x) > _stateMachine.Stats.MoveThreshold) {
                SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Moving));
            } else {
                SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Idling));
            }
            return;
        }
    }

    public override void ExitState() {

        // Ensure abilities are re-enabled
        _stateMachine.Blackboard.CanJump = true;
    }
}
