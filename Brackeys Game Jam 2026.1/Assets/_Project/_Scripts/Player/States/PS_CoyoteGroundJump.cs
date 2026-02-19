using UnityEngine;

/// <summary>
/// Airborne substate: Coyote-time ground jump.
/// Allows the player to jump shortly after walking off a ledge.
/// Immediately transitions to GroundJumping after applying force.
/// </summary>
public class PS_CoyoteGroundJump : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;

    public PS_CoyoteGroundJump(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_CoyoteGroundJump] Entered - Coyote Jump!");

        _sm.Input.ConsumeJumpBuffer();
        _sm.Blackboard.CoyoteTimer = 0f;
        _sm.Blackboard.IsJumping   = true;

        _sm.Physics.ApplyVerticalForce(_sm.Stats.InitialGroundJumpVelocity);
        _sm.Animation.Play(PlayerAnimationHandler.Jump, false);

        SwitchState(_sm.GetFactory().GetState(PlayerStateFactory.PlayerStates.GroundJumping));
    }

    public override void InitializeSubState() { }
    public override void Update()             { }
    public override void FixedUpdate()        { }
    public override void CheckSwitchStates()  { }
    public override void ExitState()          { }
}
