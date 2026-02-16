using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class HierarchicalStateMachine : MonoBehaviour {
    [Header("Debug")]
    public bool debugStates = false;


    public HierarchicalStateFactory Factory { get; private set; }
    public SceneContainerSO SceneContainer { get; private set; }


    private void Start() {
        InitializeStates();

    }

    public enum States {
        // root super
        Grounded,
        // sub
        GroundJump,
        Idling,
        Moving,
        Landing,
        GroundDodging,

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
        WallRun,
        WallJump,
    }

    public void InitializeStates() {

        //_states[States.Grounded] = new CharacterGroundedState(this);
        //_states[States.Idling] = new PlayerIdlingState(this);
        //_states[States.Moving] = new PlayerMovingState(this);
        //_states[States.Falling] = new PlayerFallingState(this);
        //_states[States.GroundJump] = new PlayerGroundJumpState(this);
        //_states[States.GroundDodging] = new CharacterDodgingState(this);


        //_states[States.Airborne] = new PlayerAirborneState(this);
        //_states[States.Falling] = new PlayerFallingState(this);
        //_states[States.GroundJumping] = new PlayerGroundJumpingState(this);
        //_states[States.AirHanging] = new PlayerAirHangingState(this);

        //_states[States.OnWall] = new PlayerOnWallState(this);
    }

    public void SetState(BaseHierarchicalState i_state) {

    }

}
