using UnityEngine;

/// <summary>
/// Airborne substate: Coyote-time wall jump.
/// Allows the player to wall jump shortly after leaving a wall.
/// Immediately transitions to WallJumping after applying force.
/// </summary>
public class PS_CoyoteWallJump : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_CoyoteWallJump(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_CoyoteWallJump] Entered - Coyote Wall Jump!");

        _sm.Input.ConsumeJumpBuffer();
        _sm.Blackboard.IsWallJumping = true;

        int wallDir = _sm.Blackboard.WallDirection;
        if (wallDir == 0) {
            // Fallback: use opposite of facing direction
            wallDir = _sm.Blackboard.IsFacingRight ? 1 : -1;
            if (_sm.Blackboard.debugStates)
                Debug.LogWarning("[PS_CoyoteWallJump] WallDirection was 0 — using fallback.");
        }

        float hVel = -wallDir * _sm.Stats.WallJumpDirection.x;
        float vVel = _sm.Stats.InitialWallJumpVelocity;
        _sm.Blackboard.Velocity    = new Vector2(hVel, vVel);
        _sm.Blackboard.IsFacingRight = wallDir < 0; // Face away from the wall

        if (_sm.Blackboard.debugStates)
            Debug.Log($"[PS_CoyoteWallJump] Wall {(wallDir > 0 ? "RIGHT" : "LEFT")} → " +
                      $"velocity ({hVel:F2}, {vVel:F2})");

        _sm.Animation.Play(PlayerAnimationHandler.Jump, false);

        SwitchState(_sm.GetFactory().GetState(PlayerStateFactory.PlayerStates.WallJumping));
    }

    public override void InitializeSubState() { }
    public override void Update()             { }
    public override void FixedUpdate()        { }
    public override void CheckSwitchStates()  { }
    public override void ExitState()          { }
}
