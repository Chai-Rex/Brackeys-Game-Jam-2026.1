using UnityEngine;

/// <summary>
/// Root state: Player is airborne
/// Manages air-based substates (Falling, GroundJumping, AirHanging, etc.)
/// </summary>
public class PS_Airborne : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_Airborne(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
        _isRootState = true;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_Airborne] Entered");
        }

        _stateMachine.Blackboard.IsGravityDisabled = false;
    }

    public override void InitializeSubState() {
        var factory = _stateMachine.GetFactory();

        // Determine which air state to start in
        if (_stateMachine.Blackboard.IsJumping) {
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.GroundJumping));
        } else if (_stateMachine.Blackboard.IsWallJumping) {
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.WallJumping));
        } else {
            // Default to falling
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
        }
    }

    public override void Update() {
        // Root state update logic
    }

    public override void FixedUpdate() {

    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check if player landed
        if (_stateMachine.Blackboard.IsGrounded && 
            !_stateMachine.Blackboard.IsJumping && 
            !_stateMachine.Blackboard.IsWallJumping) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Grounded));
            return;
        }

        // Check if player touched a wall (and not grounded)
        if (_stateMachine.Blackboard.IsAgainstWall &&
            !_stateMachine.Blackboard.IsGrounded &&
            _stateMachine.Blackboard.Velocity.y <= 0 && // Only if moving down or stationary
            !_stateMachine.Blackboard.IsWallJumping) 
        {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.OnWall));
            return;
        }
    }

    public override void ExitState() {

    }
}
