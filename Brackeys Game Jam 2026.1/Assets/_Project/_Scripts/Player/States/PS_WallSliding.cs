using UnityEngine;

/// <summary>
/// OnWall substate: Player is sliding down a wall.
/// Applies reduced downward gravity for a controlled slide feel.
/// </summary>
public class PS_WallSliding : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_WallSliding(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_WallSliding] Entered");

        _sm.Blackboard.IsFacingRight = _sm.Blackboard.WallDirection > 0; // Face the wall
        _sm.Animation.Play(PlayerAnimationHandler.WallSliding, false);
    }

    public override void InitializeSubState() { }

    public override void Update() { }

    public override void FixedUpdate() {
        // Only apply slide physics when moving downward (don't stick on upward arc)
        if (_sm.Blackboard.Velocity.y <= 0) {
            _sm.Physics.ApplyWallSlide(
                _sm.Stats.WallSlideTargetSpeed,
                _sm.Stats.WallSlideDeceleration);
        }

        // Allow the player to move horizontally away from the wall
        _sm.Physics.ApplyHorizontalMovement(
            _sm.Stats.GroundTargetSpeed,
            _sm.Stats.GroundAcceleration,
            _sm.Stats.GroundDeceleration);
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        if (_sm.Blackboard.JumpBufferTimer > 0 && _sm.Blackboard.CanJump) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.WallJump));
            return;
        }

        // Pushing away from wall — parent OnWall will detect loss of IsAgainstWall
        // and transition to Airborne automatically; nothing to do here.
    }

    public override void ExitState() { }
}
