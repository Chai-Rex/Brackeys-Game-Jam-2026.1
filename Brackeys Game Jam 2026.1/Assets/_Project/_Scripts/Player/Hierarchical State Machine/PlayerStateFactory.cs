using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and caches all player states.
/// States are singletons within a session — the same object is reused each time
/// a state is entered, so EnterState() must always fully reset instance variables.
/// </summary>
public class PlayerStateFactory : IStateFactory<PlayerStateFactory.PlayerStates> {

    public enum PlayerStates {
        // ── Root States ───────────────────────────────────────────────
        Grounded,
        Airborne,
        OnWall,

        // ── Grounded Substates ────────────────────────────────────────
        Idling,
        Moving,
        GroundedJump,
        Landing,
        Dodging,
        GroundedTurning,
        GroundedWallPressing,

        // ── Airborne Substates ────────────────────────────────────────
        Falling,
        GroundJumping,
        AirHanging,
        CoyoteGroundJump,
        CoyoteWallJump,
        WallJumping,
        AirDodging,

        // ── OnWall Substates ──────────────────────────────────────────
        WallSliding,
        WallJump,
    }

    private readonly PlayerStateMachineHandler _context;
    private readonly Dictionary<PlayerStates, BaseHierarchicalState> _states;

    public PlayerStateFactory(PlayerStateMachineHandler context) {
        _context = context;
        _states  = new Dictionary<PlayerStates, BaseHierarchicalState>();
    }

    public void InitializeStates() {
        // Root states
        _states[PlayerStates.Grounded] = new PS_Grounded(_context);
        _states[PlayerStates.Airborne] = new PS_Airborne(_context);
        _states[PlayerStates.OnWall]   = new PS_OnWall(_context);

        // Grounded substates
        _states[PlayerStates.Idling]               = new PS_Idling(_context);
        _states[PlayerStates.Moving]               = new PS_Moving(_context);
        _states[PlayerStates.GroundedJump]         = new PS_GroundedJump(_context);
        _states[PlayerStates.Landing]              = new PS_Landing(_context);
        _states[PlayerStates.Dodging]              = new PS_Dodging(_context);
        _states[PlayerStates.GroundedTurning]      = new PS_GroundedTurning(_context);
        _states[PlayerStates.GroundedWallPressing] = new PS_GroundedWallPressing(_context);

        // Airborne substates
        _states[PlayerStates.Falling]          = new PS_Falling(_context);
        _states[PlayerStates.GroundJumping]    = new PS_GroundJumping(_context);
        _states[PlayerStates.AirHanging]       = new PS_AirHanging(_context);
        _states[PlayerStates.CoyoteGroundJump] = new PS_CoyoteGroundJump(_context);
        _states[PlayerStates.CoyoteWallJump]   = new PS_CoyoteWallJump(_context);
        _states[PlayerStates.WallJumping]      = new PS_WallJumping(_context);
        _states[PlayerStates.AirDodging]       = new PS_AirDodging(_context);

        // OnWall substates
        _states[PlayerStates.WallSliding] = new PS_WallSliding(_context);
        _states[PlayerStates.WallJump]    = new PS_WallJump(_context);
    }

    public void SetState(PlayerStates state) {
        var s = GetState(state);
        if (s == null) return;

        _context.SetState(s);
        s.EnterState();
        s.InitializeSubState();
    }

    public BaseHierarchicalState GetState(PlayerStates state) {
        if (_states.TryGetValue(state, out var s)) return s;

        Debug.LogError($"[PlayerStateFactory] State '{state}' not found!");
        return null;
    }
}
