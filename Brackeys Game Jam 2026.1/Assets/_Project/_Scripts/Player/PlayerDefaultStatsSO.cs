using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDefaultStats", menuName = "ScriptableObjects/Player/PlayerDefaultStats")]
public class PlayerDefaultStatsSO : ScriptableObject {
    #region Movement Stats

    [Header("Ground Movement")]
    [Range(0f, 1f)] public float MoveThreshold = 0.1f;
    [Range(1f, 100f)] public float GroundTargetSpeed = 12.5f;
    [Range(1f, 100f)] public float GroundMaxSpeed = 18f;
    [Range(0.1f, 100f)] public float GroundAcceleration = 5f;
    [Range(0.1f, 100f)] public float GroundDeceleration = 20f;

    [Header("Airborne Movement")]
    [Range(1f, 100f)] public float AirborneTargetSpeed = 12.5f;
    [Range(0.1f, 100f)] public float AirborneAcceleration = 5f;
    [Range(0.1f, 100f)] public float AirborneDeceleration = 5f;

    [Header("Wall Sliding")]
    [Min(0.01f)] public float WallSlideTargetSpeed = 5f;
    [Range(0.0f, 100f)] public float WallSlideDeceleration = 10f;

    [Header("Turning")]
    [Range(0.1f, 100f)] public float TurnThreshold = 8f;
    [Range(0.1f, 100f)] public float TurnDeceleration = 4f;
    [Range(0f, 1f)] public float TurnBuffer = 0.1f;

    #endregion

    #region Jump Stats

    [Header("Jump Timing")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.1f;
    [Range(0f, 1f)] public float CoyoteTime = 0.1f;

    [Header("Jump Apex (Hang Time)")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;
    [Range(1f, 100f)] public float ApexHangTargetSpeed = 12.5f;
    [Range(0.1f, 100f)] public float ApexHangAcceleration = 5f;
    [Range(0.1f, 100f)] public float ApexHangDeceleration = 5f;

    [Header("Ground Jump")]
    public float GroundJumpHeight = 5f;
    public float GroundJumpTimeTillApex = 0.35f;
    [Range(0.01f, 1f)] public float GroundJumpCancelableTime = 0.028f;
    [Range(0.01f, 5f)] public float GroundJumpReleaseGravityMultiplier = 1.0f;

    [Header("Air Jump")]
    public float AirJumpHeight = 5f;
    public float AirJumpTimeTillApex = 0.35f;
    [Range(0.01f, 1f)] public float AirJumpCancelableTime = 0.028f;
    [Range(0.01f, 5f)] public float AirJumpReleaseGravityMultiplier = 1.0f;

    [Header("Wall Jump")]
    public Vector2 WallJumpDirection = new Vector2(20f, 6.5f);
    public float WallJumpTimeTillApex = 0.35f;
    [Range(0.01f, 5f)] public float WallJumpCancelableTime = 0.028f;
    [Range(0.01f, 5f)] public float WallJumpReleaseGravityMultiplier = 1.0f;

    #endregion

    #region Physics Stats

    [Header("Physics")]
    public float MaxFallSpeed = 20f;
    [Range(0.1f, 100f)] public float DecelerationAfterForce = 5f;

    #endregion

    #region Collision Stats

    [Header("Collision Detection")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetectionRayLength = 0.02f;
    public float HeadWidth = 0.75f;
    public float WallDetectionRayLength = 0.02f;
    [Range(0.01f, 2f)] public float WallDetectionRayHeightMultiplier = 0.9f;

    #endregion

    #region Special Abilities

    [Header("Dodging")]
    public float DodgingCooldown = 0.25f;
    public float DodgingDistance = 5f;

    #endregion

}
