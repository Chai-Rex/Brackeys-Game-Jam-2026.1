using System.Collections.Generic;
using UnityEngine;

public class HierarchicalStateFactory : ScriptableObject {

    private enum PlayerStates {
        // root super
        Grounded,
        // sub
        GroundDamaged,
        GroundJump,
        GroundAttacking,
        Idling,
        Moving,
        Landing,
        Dodging,
        GroundTurning,

        // root super
        Airborne,
        // sub
        AirHanging,
        Falling,
        GroundJumping,
        CoyoteGroundJump,
        CoyoteWallJump,
        WallJumping,

        // root super
        OnWall,
        // sub
        WallSliding,
        WallJump,

    }
    private HierarchicalStateMachineSO _context;

    private Dictionary<PlayerStates, BaseHierarchicalState> _states = new Dictionary<PlayerStates, BaseHierarchicalState>();

    public HierarchicalStateFactory(HierarchicalStateMachineSO currentContext) {
        _context = currentContext;

        //_states[PlayerStates.Grounded] = new PlayerGroundedState(_context, this);
        //_states[PlayerStates.Airborne] = new PlayerAirborneState(_context, this);
        //_states[PlayerStates.OnWall] = new PlayerOnWallState(_context, this);

        //_states[PlayerStates.Idling] = new PlayerIdlingState(_context, this);
        //_states[PlayerStates.Moving] = new PlayerMovingState(_context, this);
        //_states[PlayerStates.GroundAttacking] = new PlayerGroundAttackingState(_context, this);
        //_states[PlayerStates.Falling] = new PlayerFallingState(_context, this);
        //_states[PlayerStates.Landing] = new PlayerLandingState(_context, this);
        //_states[PlayerStates.GroundJump] = new PlayerGroundJumpState(_context, this);
        //_states[PlayerStates.GroundJumping] = new PlayerGroundJumpingState(_context, this);
        //_states[PlayerStates.AirHanging] = new PlayerAirHangingState(_context, this);
        //_states[PlayerStates.WallSliding] = new PlayerWallSlidingState(_context, this);
        //_states[PlayerStates.CoyoteGroundJump] = new PlayerCoyoteGroundJumpState(_context, this);
        //_states[PlayerStates.CoyoteWallJump] = new PlayerCoyoteWallJumpState(_context, this);
        //_states[PlayerStates.WallJump] = new PlayerWallJumpState(_context, this);
        //_states[PlayerStates.WallJumping] = new PlayerWallJumpingState(_context, this);
        //_states[PlayerStates.GroundTurning] = new PlayerGroundTurningState(_context, this);
    }

    public BaseHierarchicalState Grounded() { return _states[PlayerStates.Grounded]; }
    public BaseHierarchicalState Airborne() { return _states[PlayerStates.Airborne]; }
    public BaseHierarchicalState Idling() { return _states[PlayerStates.Idling]; }
    public BaseHierarchicalState Moving() { return _states[PlayerStates.Moving]; }
    public BaseHierarchicalState GroundAttacking() { return _states[PlayerStates.GroundAttacking]; }
    public BaseHierarchicalState Falling() { return _states[PlayerStates.Falling]; }
    public BaseHierarchicalState Landing() { return _states[PlayerStates.Landing]; }
    public BaseHierarchicalState GroundJump() { return _states[PlayerStates.GroundJump]; }
    public BaseHierarchicalState GroundJumping() { return _states[PlayerStates.GroundJumping]; }
    public BaseHierarchicalState AirHanging() { return _states[PlayerStates.AirHanging]; }
    public BaseHierarchicalState OnWall() { return _states[PlayerStates.OnWall]; }
    public BaseHierarchicalState WallSliding() { return _states[PlayerStates.WallSliding]; }
    public BaseHierarchicalState CoyoteGroundJump() { return _states[PlayerStates.CoyoteGroundJump]; }
    public BaseHierarchicalState CoyoteWallJump() { return _states[PlayerStates.CoyoteWallJump]; }
    public BaseHierarchicalState WallJump() { return _states[PlayerStates.WallJump]; }
    public BaseHierarchicalState WallJumping() { return _states[PlayerStates.WallJumping]; }
    public BaseHierarchicalState GroundTurning() { return _states[PlayerStates.GroundTurning]; }



}
