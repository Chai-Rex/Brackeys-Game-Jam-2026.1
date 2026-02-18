using UnityEngine;

/// <summary>
/// Airborne substate: Player is at the apex of a jump (hang time)
/// Provides a brief moment of reduced gravity for better jump feel
/// </summary>
public class PS_AirHanging : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;
    private float _hangTimer;

    public PS_AirHanging(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_AirHanging] Entered - Apex Hang Time");
        }

        _hangTimer = 0f;

        // Play hang animation
        _stateMachine.Animation.Play(PlayerAnimamationHandler.AirHanging, false);

        // IMPORTANT: Set vertical velocity to near-zero for hang effect
        _stateMachine.Blackboard.Velocity.y = 0f;
    }

    public override void InitializeSubState() {
        // AirHanging has no substates
    }

    public override void Update() {
        _hangTimer += Time.deltaTime;
    }

    public override void FixedUpdate() {
        // Apply MINIMAL gravity during hang time for "floaty" feel
        // This is what makes the hang time feel good!
        if (_hangTimer < _stateMachine.Stats.ApexHangTime) {
            // Very light gravity during hang period (30% of normal)
            float reducedGravity = _stateMachine.Stats.GroundJumpGravity * 0.3f;
            _stateMachine.Physics.ApplyGravityForce(reducedGravity);
        } else {
            // After hang time expires, start falling with a small initial velocity
            _stateMachine.Blackboard.Velocity.y = -0.1f;
        }

        // Allow full air control during hang time
        _stateMachine.Physics.ApplyHorizontalMovement(
            _stateMachine.Stats.ApexHangTargetSpeed,
            _stateMachine.Stats.ApexHangAcceleration,
            _stateMachine.Stats.ApexHangDeceleration
        );

        // Update facing direction
        _stateMachine.CheckForTurning(_stateMachine.Blackboard.MoveInput);
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Exit hang state after hang duration OR if velocity increases significantly
        if (_hangTimer >= _stateMachine.Stats.ApexHangTime ||
            _stateMachine.Blackboard.Velocity.y < -1f) {
            // Transition to falling
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Falling));
            return;
        }

        // Note: Could add double jump here if desired
        // if (_stateMachine.CheckJump() && _hasDoubleJump) { ... }
    }

    public override void ExitState() {


    }
}