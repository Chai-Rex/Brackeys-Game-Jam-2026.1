using UnityEngine;

/// <summary>
/// Root state: Player is grounded
/// Manages ground-based substates (Idling, Moving, GroundedJump, etc.)
/// </summary>
public class PS_Grounded : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_Grounded(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
        _isRootState = true;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_Grounded] Entered");
        }

        // Enable ground-based movement
        _stateMachine.Blackboard.IsGravityDisabled = false;
        _stateMachine.Blackboard.OnLanded();
    }

    public override void InitializeSubState() {
        // Default to Idling when entering grounded state
        var factory = _stateMachine.GetFactory();

        // Check velocity to determine initial substate
        if (Mathf.Abs(_stateMachine.Blackboard.Velocity.x) > _stateMachine.Stats.MoveThreshold) {
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.Moving));
        } else {
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.Idling));
        }
    }

    public override void Update() {
        // Root state update logic
    }

    public override void FixedUpdate() {
        // Apply gravity when grounded (keeps player stuck to ground)
        _stateMachine.Blackboard.Velocity.y = -1f;
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check if player left the ground
        if (!_stateMachine.Blackboard.IsGrounded ||
            _stateMachine.Blackboard.IsJumping ||
            _stateMachine.Blackboard.IsWallJumping) {
            // Give coyote time
            _stateMachine.Blackboard.CoyoteTimer = _stateMachine.Stats.CoyoteTime;
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Airborne));
        }
    }

    public override void ExitState() {

    }
}

