using UnityEngine;

/// <summary>
/// Root state: Player is airborne (not touching the ground or a wall).
/// Manages air-based substates: Falling, GroundJumping, WallJumping, AirHanging, etc.
/// </summary>
public class PS_Airborne : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_Airborne(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
        _isRootState = true;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_Airborne] Entered");

        _sm.Blackboard.IsGravityDisabled = false;
    }

    public override void InitializeSubState() {
        var factory = _sm.GetFactory();

        if (_sm.Blackboard.IsJumping)
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.GroundJumping));
        else if (_sm.Blackboard.IsWallJumping)
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.WallJumping));
        else
            SetSubState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
    }

    public override void Update() { }

    public override void FixedUpdate() { }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        // Land on ground (only valid when not mid-jump to avoid wall-top false positives)
        if (_sm.Blackboard.IsGrounded &&
            !_sm.Blackboard.IsJumping &&
            !_sm.Blackboard.IsWallJumping) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Grounded));
            return;
        }

        // Touch a wall (moving downward or neutral, with horizontal input, not jumping)
        if (_sm.Blackboard.IsAgainstWall &&
            !_sm.Blackboard.IsGrounded &&
            !_sm.Blackboard.IsJumping &&
            !_sm.Blackboard.IsWallJumping &&
            _sm.Blackboard.Velocity.y <= 0 &&
            Mathf.Abs(_sm.Blackboard.MoveInput.x) > _sm.Stats.MoveThreshold) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.OnWall));
            return;
        }
    }

    public override void ExitState() { }
}