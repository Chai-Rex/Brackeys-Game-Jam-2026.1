using System;
using UnityEngine;

/// <summary>
/// Stores all player stats and configuration values
/// Can be loaded from a ScriptableObject for easy tweaking
/// </summary>
public class PlayerStatsHandler : MonoBehaviour {
    [Header("Default Stats Reference")]
    [SerializeField] private PlayerDefaultStatsSO _defaultStats;

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

    #region Calculated Values

    // Ground Jump
    public float GroundJumpGravity { get; private set; }
    public float InitialGroundJumpVelocity { get; private set; }

    // Air Jump
    public float AirJumpGravity { get; private set; }
    public float InitialAirJumpVelocity { get; private set; }

    // Wall Jump
    public float WallJumpGravity { get; private set; }
    public float InitialWallJumpVelocity { get; private set; }

    #endregion

    #region Initialization

    private void OnValidate() {
        CalculateJumpValues();
    }

    private void OnEnable() {
        CalculateJumpValues();
    }

    /// <summary>
    /// Load stats from a ScriptableObject preset
    /// </summary>
    public void LoadFromDefaultStats(PlayerDefaultStatsSO defaultStats) {
        if (defaultStats == null) {
            Debug.LogWarning("PlayerStatsHandler: No default stats provided!");
            return;
        }

        _defaultStats = defaultStats;

        // Copy all values from the ScriptableObject
        MoveThreshold = defaultStats.MoveThreshold;
        MaxFallSpeed = defaultStats.MaxFallSpeed;

        // Ground Movement
        GroundTargetSpeed = defaultStats.GroundTargetSpeed;
        GroundMaxSpeed = defaultStats.GroundMaxSpeed;
        GroundAcceleration = defaultStats.GroundAcceleration;
        GroundDeceleration = defaultStats.GroundDeceleration;

        // Air Movement
        AirborneTargetSpeed = defaultStats.AirborneTargetSpeed;
        AirborneAcceleration = defaultStats.AirborneAcceleration;
        AirborneDeceleration = defaultStats.AirborneDeceleration;

        // Wall Movement
        WallSlideTargetSpeed = defaultStats.WallSlideTargetSpeed;
        WallSlideDeceleration = defaultStats.WallSlideDeceleration;

        // Turning
        TurnThreshold = defaultStats.TurnThreshold;
        TurnDeceleration = defaultStats.TurnDeceleration;
        TurnBuffer = defaultStats.TurnBuffer;

        // Jump Timing
        JumpBufferTime = defaultStats.JumpBufferTime;
        CoyoteTime = defaultStats.CoyoteTime;
        ApexThreshold = defaultStats.ApexThreshold;
        ApexHangTime = defaultStats.ApexHangTime;
        ApexHangTargetSpeed = defaultStats.ApexHangTargetSpeed;
        ApexHangAcceleration = defaultStats.ApexHangAcceleration;
        ApexHangDeceleration = defaultStats.ApexHangDeceleration;
        
        // Ground Jump
        GroundJumpHeight = defaultStats.GroundJumpHeight;
        GroundJumpTimeTillApex = defaultStats.GroundJumpTimeTillApex;
        GroundJumpCancelableTime = defaultStats.GroundJumpCancelableTime;
        GroundJumpReleaseGravityMultiplier = defaultStats.GroundJumpReleaseGravityMultiplier;

        // Air Jump
        AirJumpHeight = defaultStats.AirJumpHeight;
        AirJumpTimeTillApex = defaultStats.AirJumpTimeTillApex;
        AirJumpCancelableTime = defaultStats.AirJumpCancelableTime;
        AirJumpReleaseGravityMultiplier = defaultStats.AirJumpReleaseGravityMultiplier;

        // Wall Jump
        WallJumpDirection = defaultStats.WallJumpDirection;
        WallJumpTimeTillApex = defaultStats.WallJumpTimeTillApex;
        WallJumpCancelableTime = defaultStats.WallJumpCancelableTime;
        WallJumpReleaseGravityMultiplier = defaultStats.WallJumpReleaseGravityMultiplier;

        // Dodging
        DodgingCooldown = defaultStats.DodgingCooldown;
        DodgingDistance = defaultStats.DodgingDistance;

        // Collision
        GroundLayer = defaultStats.GroundLayer;
        GroundDetectionRayLength = defaultStats.GroundDetectionRayLength;
        HeadDetectionRayLength = defaultStats.HeadDetectionRayLength;
        HeadWidth = defaultStats.HeadWidth;
        WallDetectionRayLength = defaultStats.WallDetectionRayLength;
        WallDetectionRayHeightMultiplier = defaultStats.WallDetectionRayHeightMultiplier;

        CalculateJumpValues();
    }

    /// <summary>
    /// Calculate jump physics values based on desired height and time
    /// Uses kinematic equations: v = gt, h = 0.5gt^2
    /// </summary>
    private void CalculateJumpValues() {
        // Ground Jump: Calculate gravity and initial velocity
        GroundJumpGravity = -(2f * GroundJumpHeight) / Mathf.Pow(GroundJumpTimeTillApex, 2f);
        InitialGroundJumpVelocity = Mathf.Abs(GroundJumpGravity) * GroundJumpTimeTillApex;

        // Air Jump
        AirJumpGravity = -(2f * AirJumpHeight) / Mathf.Pow(AirJumpTimeTillApex, 2f);
        InitialAirJumpVelocity = Mathf.Abs(AirJumpGravity) * AirJumpTimeTillApex;

        // Wall Jump
        WallJumpGravity = -(2f * WallJumpDirection.y) / Mathf.Pow(WallJumpTimeTillApex, 2f);
        InitialWallJumpVelocity = Mathf.Abs(WallJumpGravity) * WallJumpTimeTillApex;
    }

    #endregion

    /// <summary>
    /// Reset to default stats
    /// </summary>
    public void ResetToDefaults() {
        if (_defaultStats != null) {
            LoadFromDefaultStats(_defaultStats);
        }
    }
    
    //

    private void Awake()
    {
        SkillTreeNode.upgradeActivation += ApplyUpgrade;
    }

    private void OnDestroy()
    {
        SkillTreeNode.upgradeActivation -= ApplyUpgrade;
    }

    public void ApplyUpgrade(SkillUpgradeSO upgradeSO)
    {
        foreach (var dataObject in upgradeSO.SkillUpgrades)
        {
            //Find the field according to the name
            var field = GetType().GetField(dataObject.SkillUpgradeEnum.ToString());
            
            if (field != null && field.FieldType == typeof(float))
            {
                //Add the current value of the field with the upgrade amount
                float current = (float)field.GetValue(this);
                field.SetValue(this, current + dataObject.UpgradeAmount);
                Debug.Log(field.Name+" has been upgraded to "+field.GetValue(this)+" from "+current);
            }
        }
        
        CalculateJumpValues();
    }
}