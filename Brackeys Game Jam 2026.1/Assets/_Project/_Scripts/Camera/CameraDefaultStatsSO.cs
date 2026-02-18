using UnityEngine;

[CreateAssetMenu(fileName = "CameraDefaultStatsSO", menuName = "ScriptableObjects/Camera/CameraDefaultStatsSO")]
public class CameraDefaultStatsSO : ScriptableObject {

    [Header("Follow Point Override")]
    [Tooltip("If true, overrides the CameraFollowPlayer transform's local position on initialize")]
    public bool OverrideFollowPointLocalPosition = false;
    public Vector2 FollowPointLocalPositionOverride = Vector2.zero;

    [Header("Damping")]
    [Tooltip("How smoothly the camera follows on the X axis. Higher = slower/smoother")]
    [Range(0f, 1f)] public float DampingX = 0.15f;
    [Tooltip("How smoothly the camera follows on the Y axis. Higher = slower/smoother")]
    [Range(0f, 1f)] public float DampingY = 0.25f;
    [Tooltip("Faster damping used when snapping to a landing position after a fall")]
    [Range(0f, 1f)] public float LandingSettleDamping = 0.05f;

    [Header("Deadzone")]
    [Tooltip("The camera won't move until the follow point exits this box (in world units)")]
    public Vector2 DeadzoneSize = new Vector2(1.5f, 0.8f);

    [Header("Axis Lock")]
    public bool LockHorizontal = false;
    public bool LockVertical = false;

    [Header("Lookahead -- Horizontal")]
    [Tooltip("Minimum horizontal speed before any horizontal lookahead is applied (world units/s)")]
    public float LookaheadMinVelocityX = 0.5f;
    [Tooltip("Horizontal speed at which the maximum lookahead distance is reached (world units/s)")]
    public float LookaheadMaxVelocityX = 12f;
    [Tooltip("Minimum lookahead offset applied once MinVelocityX is exceeded (world units)")]
    public float LookaheadMinDistanceX = 0.5f;
    [Tooltip("Maximum lookahead offset at MaxVelocityX (world units)")]
    public float LookaheadMaxDistanceX = 4f;
    [Tooltip("Controls how horizontal lookahead scales between min and max velocity. " +
             "X axis = normalized velocity (0 to 1), Y axis = normalized offset (0 to 1).")]
    public AnimationCurve LookaheadCurveX = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("How quickly the horizontal lookahead moves toward its target")]
    [Range(0f, 1f)] public float LookaheadDampingX = 0.08f;
    [Tooltip("How quickly horizontal lookahead retracts when the player slows or reverses")]
    [Range(0f, 1f)] public float LookaheadRetractDampingX = 0.04f;

    [Header("Lookahead -- Vertical (Falling / Down)")]
    [Tooltip("Only active while airborne and moving downward. " +
             "Minimum downward speed before the camera leads downward (world units/s)")]
    public float LookaheadMinVelocityYDown = 3f;
    [Tooltip("Downward speed at which the maximum downward lead distance is reached (world units/s)")]
    public float LookaheadMaxVelocityYDown = 14f;
    [Tooltip("Minimum downward lead offset applied once MinVelocityYDown is exceeded (world units)")]
    public float LookaheadMinDistanceYDown = 0.5f;
    [Tooltip("Maximum downward lead offset at MaxVelocityYDown (world units)")]
    public float LookaheadMaxDistanceYDown = 3f;
    [Tooltip("Curve for downward lookahead. X = normalised speed (0-1), Y = normalised offset (0-1)")]
    public AnimationCurve LookaheadCurveYDown = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Lookahead -- Vertical (Rising / Up)")]
    [Tooltip("Only active while airborne and moving upward (e.g. rising out of a dig shaft). " +
             "Set high enough that a normal jump does not trigger it (world units/s)")]
    public float LookaheadMinVelocityYUp = 8f;
    [Tooltip("Upward speed at which the maximum upward lead distance is reached (world units/s)")]
    public float LookaheadMaxVelocityYUp = 20f;
    [Tooltip("Minimum upward lead offset applied once MinVelocityYUp is exceeded (world units)")]
    public float LookaheadMinDistanceYUp = 0.5f;
    [Tooltip("Maximum upward lead offset at MaxVelocityYUp (world units)")]
    public float LookaheadMaxDistanceYUp = 3f;
    [Tooltip("Curve for upward lookahead. X = normalised speed (0-1), Y = normalised offset (0-1)")]
    public AnimationCurve LookaheadCurveYUp = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Lookahead -- Vertical (Shared)")]
    [Tooltip("How quickly the vertical lookahead offset moves toward its target")]
    [Range(0f, 1f)] public float LookaheadDampingY = 0.1f;
    [Tooltip("How quickly the vertical lookahead offset retracts when speed drops or direction changes")]
    [Range(0f, 1f)] public float LookaheadRetractDampingY = 0.05f;

    [Header("Jump Vertical Lock")]
    [Tooltip("While airborne, the camera holds its Y unless the follow point rises above this distance (world units)")]
    public float JumpVerticalLockBoundsTop = 2f;
    [Tooltip("While airborne, the camera holds its Y unless the follow point falls below this distance (world units)")]
    public float JumpVerticalLockBoundsBottom = 3f;

    [Header("Sphere of Influence")]
    [Tooltip("Maximum fraction the camera can be pulled toward an influence object. 0 = none, 1 = full snap")]
    [Range(0f, 1f)] public float InfluenceMaxWeight = 0.4f;
    [Tooltip("How smoothly the camera blends toward influence targets")]
    [Range(0f, 1f)] public float InfluenceDamping = 0.1f;

    [Header("Camera Bounds")]
    [Tooltip("If true, clamps the camera position within WorldBounds")]
    public bool UseCameraBounds = false;
    public Bounds WorldBounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 0f));

    [Header("Screen Shake -- Trauma System")]
    [Tooltip("Maximum positional shake offset in world units")]
    public float ShakeMaxOffset = 0.3f;
    [Tooltip("Maximum rotational shake in degrees")]
    public float ShakeMaxRotation = 1.5f;
    [Tooltip("How quickly trauma decays per second")]
    public float TraumaDecayRate = 1.2f;
    [Tooltip("Frequency of the Perlin noise used for shake sampling")]
    public float ShakeFrequency = 25f;

    [Header("Crouch Mouse Look")]
    [Tooltip("How far toward the mouse cursor the camera shifts while crouching. 0 = no shift, 1 = fully on mouse.")]
    [Range(0f, 1f)] public float MouseLookWeight = 0.4f;
    [Tooltip("How smoothly the camera moves toward the mouse look target while crouching")]
    [Range(0f, 1f)] public float MouseLookDamping = 0.08f;
    [Tooltip("How smoothly the camera retracts back when crouch is released")]
    [Range(0f, 1f)] public float MouseLookRetractDamping = 0.05f;

    [Header("Debug Gizmos")]
    public bool ShowDeadzone = true;
    public bool ShowLookaheadPoint = true;
    public bool ShowCenterCrosshair = true;
    [Tooltip("Half-length of the center crosshair arms in world units")]
    public float CrosshairSize = 1f;
}