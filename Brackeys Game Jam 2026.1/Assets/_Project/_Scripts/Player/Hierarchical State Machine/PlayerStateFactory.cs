using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFactory : IStateFactory<PlayerStateFactory.PlayerStates> {
    public enum PlayerStates {
        // Root super states
        Grounded,
        Airborne,
        OnWall,

        // Grounded substates
        Idling,
        Moving,
        GroundJump,
        GroundAttacking,
        GroundDamaged,
        Landing,
        Dodging,
        GroundTurning,

        // Airborne substates
        Falling,
        GroundJumping,
        AirHanging,
        CoyoteGroundJump,
        CoyoteWallJump,
        WallJumping,

        // OnWall substates
        WallSliding,
        WallJump,
    }

    private PlayerStateMachineSO _context;
    private SceneContainerSO _sceneContainer;
    private Dictionary<PlayerStates, BaseHierarchicalState> _states;

    public PlayerStateFactory(PlayerStateMachineSO context, SceneContainerSO sceneContainer) {
        _context = context;
        _sceneContainer = sceneContainer;
        _states = new Dictionary<PlayerStates, BaseHierarchicalState>();
    }

    public void InitializeStates() {
        // Initialize all your states here
        // Root states
        _states[PlayerStates.Grounded] = new P_GroundedState(_context, _sceneContainer);
        //_states[PlayerStates.Airborne] = new PlayerAirborneState(_context, _sceneContainer);
        //_states[PlayerStates.OnWall] = new PlayerOnWallState(_context, _sceneContainer);

        // Sub states
        //_states[PlayerStates.Idling] = new PlayerIdlingState(_context, _sceneContainer);
        //_states[PlayerStates.Moving] = new PlayerMovingState(_context, _sceneContainer);
        //_states[PlayerStates.GroundAttacking] = new PlayerGroundAttackingState(_context, _sceneContainer);
        //_states[PlayerStates.Falling] = new PlayerFallingState(_context, _sceneContainer);
        //_states[PlayerStates.Landing] = new PlayerLandingState(_context, _sceneContainer);
        //_states[PlayerStates.GroundJump] = new PlayerGroundJumpState(_context, _sceneContainer);
        //_states[PlayerStates.GroundJumping] = new PlayerGroundJumpingState(_context, _sceneContainer);
        //_states[PlayerStates.AirHanging] = new PlayerAirHangingState(_context, _sceneContainer);
        //_states[PlayerStates.WallSliding] = new PlayerWallSlidingState(_context, _sceneContainer);
        //_states[PlayerStates.CoyoteGroundJump] = new PlayerCoyoteGroundJumpState(_context, _sceneContainer);
        //_states[PlayerStates.CoyoteWallJump] = new PlayerCoyoteWallJumpState(_context, _sceneContainer);
        //_states[PlayerStates.WallJump] = new PlayerWallJumpState(_context, _sceneContainer);
        //_states[PlayerStates.WallJumping] = new PlayerWallJumpingState(_context, _sceneContainer);
        //_states[PlayerStates.GroundTurning] = new PlayerGroundTurningState(_context, _sceneContainer);
    }

    public void SetState(PlayerStates state) {
        if (!_states.ContainsKey(state)) {
            Debug.LogError($"State {state} not found in factory!");
            return;
        }

        _context.SetState(_states[state]);
    }

    public BaseHierarchicalState GetState(PlayerStates state) {
        if (!_states.ContainsKey(state)) {
            Debug.LogError($"State {state} not found in factory!");
            return null;
        }

        return _states[state];
    }
}