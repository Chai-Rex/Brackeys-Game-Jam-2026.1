using UnityEngine;

/// <summary>
/// Root state: Player is on a wall (airborne, touching wall, pressing toward it).
/// Manages wall-based substates: WallSliding, WallJump.
/// </summary>
public class PS_OnWall : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_OnWall(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
        _isRootState = true;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_OnWall] Entered");

        _sm.Blackboard.IsGravityDisabled = false;
    }

    public override void InitializeSubState() {
        SetSubState(_sm.GetFactory().GetState(PlayerStateFactory.PlayerStates.WallSliding));
    }

    public override void Update() { }

    public override void FixedUpdate() { }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        if (_sm.Blackboard.IsGrounded) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Grounded));
            return;
        }

        if (!_sm.Blackboard.IsAgainstWall ||
             _sm.Blackboard.IsJumping ||
             _sm.Blackboard.IsWallJumping ||
             Mathf.Abs(_sm.Blackboard.MoveInput.x) < _sm.Stats.MoveThreshold) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Airborne));
            return;
        }
    }

    public override void ExitState() { }
}
