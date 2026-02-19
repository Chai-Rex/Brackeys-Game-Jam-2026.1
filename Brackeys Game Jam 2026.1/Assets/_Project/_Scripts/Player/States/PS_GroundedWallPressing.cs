using UnityEngine;

/// <summary>
/// Grounded substate: Player is pushing against a wall while on the ground.
/// Decelerates horizontal movement; transitions to a jump or back to Moving/Idling.
/// </summary>
public class PS_GroundedWallPressing : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_GroundedWallPressing(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_GroundedWallPressing] Entered");

        // TODO: swap for a dedicated wall-press animation when available
        _sm.Animation.Play(PlayerAnimationHandler.Idle, false);
    }

    public override void InitializeSubState() { }

    public override void Update() { }

    public override void FixedUpdate() {
        // Halt horizontal movement against the wall
        _sm.Physics.ApplyHorizontalMovement(
            0f,
            _sm.Stats.GroundAcceleration,
            _sm.Stats.GroundDeceleration * 2f);
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        if (_sm.Blackboard.JumpBufferTimer > 0 && _sm.Blackboard.CanJump) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
            return;
        }

        if (!_sm.Blackboard.IsAgainstWall) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Moving));
            return;
        }

        // Player redirected input away from the wall
        int inputDir = _sm.Blackboard.MoveInput.x > 0 ? 1 : -1;
        if (inputDir != _sm.Blackboard.WallDirection) {
            var next = _sm.CheckForInputMovement()
                ? PlayerStateFactory.PlayerStates.Moving
                : PlayerStateFactory.PlayerStates.Idling;
            SwitchState(factory.GetState(next));
            return;
        }
    }

    public override void ExitState() { }
}
