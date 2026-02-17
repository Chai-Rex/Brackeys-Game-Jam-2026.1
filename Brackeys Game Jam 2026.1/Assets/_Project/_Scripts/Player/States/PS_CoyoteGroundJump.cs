using UnityEngine;

/// <summary>
/// Airborne substate: Player performs a "coyote time" jump
/// Allows jumping shortly after leaving the ground for better game feel
/// </summary>
public class PS_CoyoteGroundJump : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_CoyoteGroundJump(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_CoyoteGroundJump] Entered - Coyote Jump!");
        }

        // Consume the jump buffer and coyote time
        _stateMachine.Input.ConsumeJumpBuffer();
        _stateMachine.Blackboard.CoyoteTimer = 0f;

        // Set jumping flag
        _stateMachine.Blackboard.IsJumping = true;

        // Apply jump force (same as ground jump)
        _stateMachine.Physics.ApplyVerticalForce(_stateMachine.Stats.InitialGroundJumpVelocity);

        // Play jump animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.Jump, false);

        // Immediately transition to ground jumping state
        var factory = _stateMachine.GetFactory();
        SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundJumping));
    }

    public override void InitializeSubState() {
        // No substates - this is a transition state
    }

    public override void Update() {
        // This state transitions immediately
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
