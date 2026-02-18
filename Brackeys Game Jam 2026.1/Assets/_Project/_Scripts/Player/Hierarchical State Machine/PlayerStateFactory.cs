using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFactory : IStateFactory<PlayerStateFactory.PlayerStates> {
    public enum PlayerStates {

        // super state
        Grounded,
        // Grounded substates
        Idling,
        Moving,
        GroundedJump,
        Landing,
        Dodging,
        GroundedTurning,
        GroundedWallPressing,

        // super state
        Airborne,
        // Airborne substates
        Falling,
        GroundJumping,
        AirHanging,
        CoyoteGroundJump,
        CoyoteWallJump,
        WallJumping,
        AirDodging,

        // super state
        OnWall, // Assumed you are only touching the wall. no ground. 
        // OnWall substates
        WallSliding,
        WallJump,
    }

    private PlayerStateMachineHandler _context;
    private Dictionary<PlayerStates, BaseHierarchicalState> _states;

    public PlayerStateFactory(PlayerStateMachineHandler context) {
        _context = context;
        _states = new Dictionary<PlayerStates, BaseHierarchicalState>();
    }

    public void InitializeStates() {
        // Initialize all states here

        // Root states
        _states[PlayerStates.Grounded] = new PS_Grounded(_context);
        _states[PlayerStates.Airborne] = new PS_Airborne(_context);
        _states[PlayerStates.OnWall] = new PS_OnWall(_context);

        // Grounded substates
        _states[PlayerStates.Idling] = new PS_Idling(_context);
        _states[PlayerStates.Moving] = new PS_Moving(_context);
        _states[PlayerStates.GroundedJump] = new PS_GroundedJump(_context);
        _states[PlayerStates.Landing] = new PS_Landing(_context);
        _states[PlayerStates.Dodging] = new PS_Dodging(_context);
        _states[PlayerStates.GroundedTurning] = new PS_GroundedTurning(_context);
        _states[PlayerStates.GroundedWallPressing] = new PS_GroundedWallPressing(_context);

        // Airborne substates
        _states[PlayerStates.Falling] = new PS_Falling(_context);
        _states[PlayerStates.GroundJumping] = new PS_GroundJumping(_context);
        _states[PlayerStates.AirHanging] = new PS_AirHanging(_context);
        _states[PlayerStates.CoyoteGroundJump] = new PS_CoyoteGroundJump(_context);
        _states[PlayerStates.CoyoteWallJump] = new PS_CoyoteWallJump(_context);
        _states[PlayerStates.WallJumping] = new PS_WallJumping(_context);
        _states[PlayerStates.AirDodging] = new PS_AirDodging(_context);

        // OnWall substates
        _states[PlayerStates.WallSliding] = new PS_WallSliding(_context);
        _states[PlayerStates.WallJump] = new PS_WallJump(_context);
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