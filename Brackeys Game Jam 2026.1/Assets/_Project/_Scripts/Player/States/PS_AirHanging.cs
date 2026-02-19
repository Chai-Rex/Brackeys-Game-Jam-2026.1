using UnityEngine;

/// <summary>
/// Airborne substate: Player is at the apex of a jump (hang time).
/// Applies reduced gravity for a brief period to give the jump a floaty, satisfying feel.
/// </summary>
public class PS_AirHanging : BaseHierarchicalState {
    private PlayerStateMachineHandler _sm;
    private float _hangTimer;

    public PS_AirHanging(PlayerStateMachineHandler stateMachine) : base(stateMachine) {
        _sm = stateMachine;
    }

    public override void EnterState() {
        if (_sm.Blackboard.debugStates) Debug.Log("[PS_AirHanging] Entered - Apex Hang");

        _hangTimer = 0f;
        _sm.Blackboard.Velocity.y = 0f; // Flatten velocity for hang feel

        _sm.Animation.Play(PlayerAnimationHandler.AirHanging, false);
    }

    public override void InitializeSubState() { }

    public override void Update() {
        _hangTimer += Time.deltaTime;
    }

    public override void FixedUpdate() {
        if (_hangTimer < _sm.Stats.ApexHangTime) {
            // Reduced gravity (30%) during the hang window
            _sm.Physics.ApplyGravityForce(_sm.Stats.GroundJumpGravity * 0.3f);
        } else {
            // Kick-start falling after hang expires
            _sm.Blackboard.Velocity.y = -0.1f;
        }

        _sm.Physics.ApplyHorizontalMovement(
            _sm.Stats.ApexHangTargetSpeed,
            _sm.Stats.ApexHangAcceleration,
            _sm.Stats.ApexHangDeceleration);
        _sm.CheckForTurning(_sm.Blackboard.MoveInput);
    }

    public override void CheckSwitchStates() {
        var factory = _sm.GetFactory();

        if (_hangTimer >= _sm.Stats.ApexHangTime || _sm.Blackboard.Velocity.y < -1f) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }
    }

    public override void ExitState() { }
}
