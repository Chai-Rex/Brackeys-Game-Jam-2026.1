
public enum SkillUpgradeEnum
{
    // Ground Movement
    MoveThreshold,
    GroundTargetSpeed,
    GroundMaxSpeed,
    GroundAcceleration,
    GroundDeceleration,

    // Airborne Movement
    AirborneTargetSpeed,
    AirborneAcceleration,
    AirborneDeceleration,

    // Wall Sliding
    WallSlideTargetSpeed,
    WallSlideDeceleration,

    // Turning
    TurnThreshold,
    TurnDeceleration,
    TurnBuffer,

    // Jump Timing
    JumpBufferTime,
    CoyoteTime,

    // Jump Apex
    ApexThreshold,
    ApexHangTime,
    ApexHangTargetSpeed,
    ApexHangAcceleration,
    ApexHangDeceleration,

    // Ground Jump
    GroundJumpHeight,
    GroundJumpTimeTillApex,
    GroundJumpCancelableTime,
    GroundJumpReleaseGravityMultiplier,

    // Air Jump
    AirJumpHeight,
    AirJumpTimeTillApex,
    AirJumpCancelableTime,
    AirJumpReleaseGravityMultiplier,

    // Wall Jump
    WallJumpDirection,
    WallJumpTimeTillApex,
    WallJumpCancelableTime,
    WallJumpReleaseGravityMultiplier,

    // Physics
    MaxFallSpeed,
    DecelerationAfterForce,

    // Collision
    GroundLayer,
    GroundDetectionRayLength,
    HeadDetectionRayLength,
    HeadWidth,
    WallDetectionRayLength,
    WallDetectionRayHeightMultiplier,

    // Abilities
    DodgingCooldown,
    DodgingDistance,
    
    //Drill
    DrillDurationNeeded
}
