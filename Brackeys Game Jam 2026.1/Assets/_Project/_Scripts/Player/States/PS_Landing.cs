using UnityEngine;

/// <summary>
/// Grounded substate: Player is landing from air
/// Brief state to handle landing animation/mechanics
/// </summary>
public class PS_Landing : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;
    private float _landingTimer;
    private const float LANDING_DURATION = 0.1f; // Brief landing state

    public PS_Landing(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_Landing] Entered");
        }

        _landingTimer = 0f;

        // Determine landing type based on fall velocity
        if (Mathf.Abs(_stateMachine.Blackboard.Velocity.y) > 15f) { // Replace Magic Number
            // Hard landing
            _stateMachine.Animation.Play(PlayerAnimamationHandler.LandingHard, false);
        } else {
            // Soft landing
            _stateMachine.Animation.Play(PlayerAnimamationHandler.LandingSoft, false);
        }

        // Reset vertical velocity on landing
        _stateMachine.Physics.StopVerticalMovement();
    }

    public override void InitializeSubState() {
        // Landing has no substates
    }

    public override void Update() {
        _landingTimer += Time.deltaTime;
    }

    public override void FixedUpdate() {
        // Apply ground deceleration during landing
        _stateMachine.Physics.ApplyHorizontalMovement(
            _stateMachine.Stats.GroundTargetSpeed,
            _stateMachine.Stats.GroundAcceleration,
            _stateMachine.Stats.GroundDeceleration * 0.5f // Reduced deceleration during landing
        );
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Allow immediate jump from landing (bunny hop)
        if (_stateMachine.Blackboard.JumpBufferTimer > 0 && _stateMachine.Blackboard.CanJump) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
            return;
        }

        // Transition out of landing state after brief duration
        if (_landingTimer >= LANDING_DURATION) {
            // Check if moving
            if (Mathf.Abs(_stateMachine.Blackboard.MoveInput.x) > _stateMachine.Stats.MoveThreshold) {
                SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Moving));
            } else {
                SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Idling));
            }
            return;
        }
    }

    public override void ExitState() {

    }
}
