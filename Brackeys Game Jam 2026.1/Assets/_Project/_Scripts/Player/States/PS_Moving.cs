using UnityEngine;

/// <summary>
/// Grounded substate: Player is moving horizontally on the ground
/// </summary>
public class PS_Moving : BaseHierarchicalState {
    private PlayerStateMachineHandler _stateMachine;

    public PS_Moving(PlayerStateMachineHandler stateMachine)
        : base(stateMachine) {
        _stateMachine = stateMachine;
    }

    public override void EnterState() {
        if (_stateMachine.Blackboard.debugStates) {
            Debug.Log("[PS_Moving] Entered");
        }

        UpdateAnimation();
    }

    public override void InitializeSubState() {
        // Moving has no substates
    }

    public override void Update() {

    }

    public override void FixedUpdate() {
        // Check for turning BEFORE applying movement
        //if (_stateMachine.CheckMovementTurn()) {
        //    // Will be handled in CheckSwitchStates
        //    return;
        //}

        // Apply ground movement
        _stateMachine.Physics.ApplyHorizontalMovement(
            _stateMachine.Stats.GroundTargetSpeed,
            _stateMachine.Stats.GroundAcceleration,
            _stateMachine.Stats.GroundDeceleration
        );

        // Update facing direction
        _stateMachine.CheckForTurning(_stateMachine.Blackboard.MoveInput);

        // Update animation based on speed
        UpdateAnimation();
    }

    public override void CheckSwitchStates() {
        var factory = _stateMachine.GetFactory();

        // Check for jump input (using reusable check)
        if (_stateMachine.CheckJump()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedJump));
            return;
        }

        // Check for turning(using reusable check)
        //if (_stateMachine.CheckMovementTurn()) {
        //    SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedTurning));
        //    return;
        //}

        // Check if stopped moving (using reusable check)
        if (_stateMachine.CheckIdling()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.Idling));
            return;
        }

        // Check if against wall while moving toward it
        if (_stateMachine.CheckPressingAgainstWall()) {
            SwitchState(factory.GetState(PlayerStateFactory.PlayerStates.GroundedWallPressing));
            return;
        }
    }

    public override void ExitState() {

    }

    private void UpdateAnimation() {
        float absVelocity = Mathf.Abs(_stateMachine.Blackboard.Velocity.x);

        // Walking animation for slow speeds
        if (absVelocity < _stateMachine.Stats.GroundTargetSpeed / 2) {
            _stateMachine.Animation.Play(PlayerAnimamationHandler.Walking, false);
        }
        // Sprinting animation for high speeds
        else if (absVelocity > _stateMachine.Stats.TurnThreshold) {
            _stateMachine.Animation.Play(PlayerAnimamationHandler.Sprinting, false);
        }
        // Running animation for normal speeds
        else {
            _stateMachine.Animation.Play(PlayerAnimamationHandler.Running, false);
        }
    }
}