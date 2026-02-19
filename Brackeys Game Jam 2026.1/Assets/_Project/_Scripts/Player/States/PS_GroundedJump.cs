using UnityEngine;

/// <summary>
/// Grounded substate: Player initiates a jump from the ground.
/// Single-frame transition state — applies jump force then immediately lets
/// PS_Grounded.CheckSwitchStates() detect IsJumping and move to Airborne.
/// </summary>
public class PS_GroundedJump : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_GroundedJump(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_GroundedJump] Entered - Executing Jump");

        _sm.Input.ConsumeJumpBuffer();
        _sm.Blackboard.IsJumping = true;
        _sm.Physics.ApplyVerticalForce(_sm.Stats.InitialGroundJumpVelocity);
        _sm.Animation.Play(PlayerAnimationHandler.Jump, false);
    }

    public override void InitializeSubState() { }
    public override void Update()             { }
    public override void FixedUpdate()        { }
    public override void CheckSwitchStates()  { }
    public override void ExitState()          { }
}
