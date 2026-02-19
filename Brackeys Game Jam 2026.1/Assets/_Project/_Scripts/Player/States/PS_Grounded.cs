using UnityEngine;

/// <summary>
/// Root state: Player is on the ground.
/// Manages ground-based substates: Idling, Moving, Landing, GroundedJump, etc.
/// </summary>
public class PS_Grounded : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_Grounded(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
        _isRootState = true;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_Grounded] Entered");

        _sm.Blackboard.IsGravityDisabled = false;
        _sm.Blackboard.OnLanded();
    }

    public override void InitializeSubState() {
        var factory = _sm.GetFactory();

        // Bunny-hop: jump buffer active on landing
        if (_sm.Blackboard.JumpBufferTimer > 0 && _sm.Blackboard.CanJump)
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
        else
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.Landing));
    }

    public override void Update() { }

    public override void FixedUpdate() {
        // Pin the player to the ground so they don't float off ramps
        if (!_sm.Blackboard.IsJumping)
            _sm.Blackboard.Velocity.y = -1f;
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        if (!_sm.Blackboard.IsGrounded ||
             _sm.Blackboard.IsJumping  ||
             _sm.Blackboard.IsWallJumping) {
            // Grant coyote time before switching to airborne
            _sm.Blackboard.CoyoteTimer = _sm.Stats.CoyoteTime;
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Airborne));
        }
    }

    public override void ExitState() { }
}
