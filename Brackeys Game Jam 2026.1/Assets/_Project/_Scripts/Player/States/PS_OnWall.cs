using UnityEngine;

/// <summary>
/// Root state: Player is on a wall (touching wall, not grounded)
/// Manages wall-based substates (WallSliding, WallJump)
/// </summary>
public class PS_OnWall : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_OnWall(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
        _isRootState = true;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_OnWall] Entered");
        }

        _stateMachine.Blackboard.IsGravityDisabled = false;
    }

    public override void InitializeSubState() {
        var factory = _stateMachine.GetFactory();

        // Default to wall sliding
        SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.WallSliding));
    }

    public override void Update() {
        // Root state update logic
    }

    public override void FixedUpdate() {
        // Apply wall sliding physics (controlled by substate)
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check if player landed
        if (_stateMachine.Blackboard.IsGrounded) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Grounded));
            return;
        }

        // Check if player left the wall
        if (!_stateMachine.Blackboard.IsAgainstWall ||
            _stateMachine.Blackboard.IsJumping ||
            _stateMachine.Blackboard.IsWallJumping) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Airborne));
            return;
        }
    }

    public override void ExitState() {

    }
}
