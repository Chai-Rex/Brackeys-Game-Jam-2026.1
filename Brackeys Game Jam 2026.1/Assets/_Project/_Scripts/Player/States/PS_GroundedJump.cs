using UnityEngine;

/// <summary>
/// Grounded substate: Player initiates jump from ground
/// This is the "jump start" state that immediately transitions to airborne
/// </summary>
public class PS_GroundedJump : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_GroundedJump(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_GroundedJump] Entered - Executing Jump");
        }

        // Consume the jump buffer
        _stateMachine.Input.ConsumeJumpBuffer();

        // Set jumping flag
        _stateMachine.Blackboard.IsJumping = true;

        // Apply jump force
        _stateMachine.Physics.ApplyVerticalForce(_stateMachine.Stats.InitialGroundJumpVelocity);

        // Play jump animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Jump, false);
    }

    public override void InitializeSubState() {
        // No substates - this is a transition state
    }

    public override void Update() {
        // This state is only active for one frame
    }

    public override void FixedUpdate() {
        // Jump force already applied in EnterState
    }

    public override void CheckSwitchStates() {
        // State switches immediately in EnterState
    }

    public override void ExitState() {

    }
}
