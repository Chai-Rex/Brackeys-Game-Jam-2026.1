using UnityEngine;

/// <summary>
/// OnWall substate: Player initiates a wall jump.
/// Single-frame transition — applies velocity away from the wall, then switches
/// to Airborne → WallJumping via the IsWallJumping flag.
/// </summary>
public class PS_WallJump : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_WallJump(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_WallJump] Entered - Executing Wall Jump");

        _sm.Input.ConsumeJumpBuffer();
        _sm.Blackboard.IsWallJumping = true;

        int wallDir = _sm.Blackboard.WallDirection;
        if (wallDir == 0) {
            Debug.LogWarning("[PS_WallJump] WallDirection is 0 — falling back to facing direction.");
            wallDir = _sm.Blackboard.IsFacingRight ? 1 : -1;
        }

        // Jump opposite to wall direction
        float hVel = -wallDir * _sm.Stats.WallJumpDirection.x;
        float vVel = _sm.Stats.InitialWallJumpVelocity;
        _sm.Blackboard.Velocity      = new Vector2(hVel, vVel);
        _sm.Blackboard.IsFacingRight = wallDir < 0; // Face away from wall

        if (_sm.Blackboard.debugStates)
            Debug.Log($"[PS_WallJump] Wall {(wallDir > 0 ? "RIGHT" : "LEFT")} → " +
                      $"velocity ({hVel:F2}, {vVel:F2})");

        _sm.Animation.Play(PlayerAnimationHandler.Jump, false);
    }

    public override void InitializeSubState() { }
    public override void Update()             { }
    public override void FixedUpdate()        { }
    public override void CheckSwitchStates()  { }
    public override void ExitState()          { }
}
