using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives the main camera using a follow point parented to the player.
///
/// All tuning values are read directly from CameraDefaultStatsSO (_stats) each frame,
/// so any change made to the SO in the Inspector is reflected immediately --
/// no restarting Play mode required.
///
/// Features
/// --------
///   Separate X / Y damping
///   Deadzone         -- camera only moves when the follow point exits a box
///   Lookahead        -- velocity-magnitude based with AnimationCurves for X and Y.
///                       Sign of velocity controls direction: fast falls lead down,
///                       fast upward jumps lead up -- one unified system for both axes.
///   Jump vertical lock -- holds camera Y while airborne, releases if player exits bounds
///   Landing settle     -- snaps to new ground Y faster after a fall
///   Sphere of influence -- external objects register a weighted pull each frame
///   Axis lock          -- freeze horizontal or vertical tracking independently
///   Camera bounds      -- clamps camera within a world-space bounding box
///   Trauma shake       -- Perlin-noise screen shake with quadratic intensity falloff
///   Crouch mouse look  -- holding crouch shifts the camera toward the mouse cursor
///
/// OnLateUpdate() must be called by CameraManager AFTER all other managers
/// so it reads the final physics positions for that frame.
/// </summary>
public class CameraHandler : MonoBehaviour {

    ////////////////////////////////////////////////////////////
    #region Serialized Fields
    ////////////////////////////////////////////////////////////

    [Header("Scene References")]
    [SerializeField] private SceneContainerSO _sceneContainer;

    [Header("Stats")]
    [SerializeField] private CameraDefaultStatsSO _stats;

    [Header("Camera")]
    [SerializeField] private Camera _camera;

    #endregion

    ////////////////////////////////////////////////////////////
    #region Private Runtime State
    ////////////////////////////////////////////////////////////

    // Camera Manager
    private CameraManager _cameraManager;

    // Player
    private PlayerHandler _playerHandler;
    private Transform _cameraFollowPlayerTransform;
    private PlayerBlackboardHandler _blackboard;

    // Input
    private InputManager _inputManager;

    // The camera's logical position this frame (screen shake is layered on top)
    private Vector3 _currentCameraPosition;

    // Crouch mouse-look: smoothed world-space offset toward the mouse cursor
    private Vector2 _currentMouseLookOffset;

    // Deadzone bounds in world space, re-centred on the camera each frame
    private Vector2 _deadzoneMin;
    private Vector2 _deadzoneMax;

    // Smoothed lookahead offsets
    private float _currentLookaheadOffsetX;
    private float _currentLookaheadOffsetY;

    // Lookahead hysteresis state per axis.
    // Activates when the follow point exits the deadzone.
    // Deactivates when velocity drops below the minimum threshold or direction reverses.
    private bool _lookaheadActiveX;
    private bool _lookaheadActiveY;

    // The committed direction (+1 or -1) at the moment lookahead was activated.
    // Direction-change cancellation compares against this, not the smoothed offset,
    // so the retracting offset itself cannot re-trigger a false cancel.
    private float _lookaheadDirectionX;
    private float _lookaheadDirectionY;

    // World-space point the camera is leading toward -- stored for gizmos
    private Vector2 _debugLookaheadTarget;

    // Jump vertical lock
    private float _jumpLockedCameraY;
    private bool _isVerticallyLocked;

    // Used to detect the frame the player lands
    private bool _wasAirborne;

    // Sphere of influence
    private readonly List<CameraInfluenceData> _activeInfluences = new List<CameraInfluenceData>();
    private Vector2 _currentInfluenceOffset;

    // Trauma / screen shake (pure runtime state, not stored in SO)
    private float _trauma;
    private float _shakeTimeX;
    private float _shakeTimeY;
    private float _shakeTimeRot;

    // Cached camera extents for bounds clamping
    private float _camHalfHeight;
    private float _camHalfWidth;

    #endregion

    ////////////////////////////////////////////////////////////
    #region Initialization
    ////////////////////////////////////////////////////////////

    private void Start() {
        _cameraManager = _sceneContainer.GetManager<CameraManager>();
        _cameraManager.SetPlayerHandler(this);
    }

    /// <summary>
    /// Called by CameraManager after the scene is ready.
    /// </summary>
    public void Initialize() {

        _inputManager = _sceneContainer.GetManager<InputManager>();
        _playerHandler = _sceneContainer.GetManager<PlayerManager>().GetPlayerHandler();
        _cameraFollowPlayerTransform = _playerHandler.CameraFollowPlayer;
        _blackboard = _playerHandler.Blackboard;

        if (_stats.OverrideFollowPointLocalPosition) {
            _cameraFollowPlayerTransform.localPosition = new Vector3(
                _stats.FollowPointLocalPositionOverride.x,
                _stats.FollowPointLocalPositionOverride.y,
                _cameraFollowPlayerTransform.localPosition.z);
        }

        // Snap to follow point instantly -- no damping on the first frame
        _currentCameraPosition = new Vector3(
            _cameraFollowPlayerTransform.position.x,
            _cameraFollowPlayerTransform.position.y,
            transform.position.z);

        transform.position = _currentCameraPosition;
        UpdateDeadzoneBounds();

        _jumpLockedCameraY = _currentCameraPosition.y;
        _isVerticallyLocked = false;
        _wasAirborne = false;

        // Randomise shake noise seeds so repeated shakes look different each session
        _shakeTimeX = Random.Range(0f, 100f);
        _shakeTimeY = Random.Range(0f, 100f);
        _shakeTimeRot = Random.Range(0f, 100f);

        if (_camera != null) {
            _camHalfHeight = _camera.orthographicSize;
            _camHalfWidth = _camHalfHeight * _camera.aspect;
        }
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region OnLateUpdate (called by CameraManager after all other managers)
    ////////////////////////////////////////////////////////////

    public void OnUpdate() {
        if (_stats == null || _cameraFollowPlayerTransform == null) return;

        float dt = Time.deltaTime;
        Vector2 followPoint = _cameraFollowPlayerTransform.position;
        Vector2 velocity = _blackboard.Velocity;
        bool isGrounded = _blackboard.IsGrounded;

        // -- 1. Velocity-based lookahead --------------------------------------
        //
        // Hysteresis rules per axis:
        //
        //   ACTIVATE X   -- follow point exits the deadzone horizontally AND
        //                   the player is grounded. A mid-air deadzone exit does
        //                   NOT activate lookahead; the player is just falling/jumping
        //                   through empty space and a sudden lead would feel wrong.
        //                   Exception: if lookahead was already active before the
        //                   player left the ground, it stays active in the air.
        //
        //   ACTIVATE Y   -- follow point exits the deadzone vertically.
        //
        //   DEACTIVATE   -- velocity magnitude drops below the minimum threshold,
        //                   OR the velocity direction flips (direction change cancels
        //                   the lead so it retracts before building up the other way).
        //                   The offset smoothly retracts via LookaheadRetractDamping.

        bool isAirborne = !isGrounded;
        bool followPointExitsDeadzoneX = followPoint.x < _deadzoneMin.x || followPoint.x > _deadzoneMax.x;
        bool followPointExitsDeadzoneY = followPoint.y < _deadzoneMin.y || followPoint.y > _deadzoneMax.y;

        // Direction-change cancellation compares current velocity against _lookaheadDirectionX/Y --
        // the committed direction captured at activation time. This avoids comparing against the
        // smoothed offset, which lags and would keep firing false cancels while retracting.
        bool directionChangedX = _lookaheadActiveX &&
                                 Mathf.Sign(velocity.x) != _lookaheadDirectionX;
        bool directionChangedY = _lookaheadActiveY &&
                                 Mathf.Sign(velocity.y) != _lookaheadDirectionY;

        // Pick the correct vertical thresholds based on travel direction.
        // Falling uses the Down set; rising uses the Up set.
        // This prevents a jump's upward velocity from triggering downward lead and vice versa.
        bool fallingNow = velocity.y < 0f;
        float minVelY = fallingNow ? _stats.LookaheadMinVelocityYDown : _stats.LookaheadMinVelocityYUp;
        float maxVelY = fallingNow ? _stats.LookaheadMaxVelocityYDown : _stats.LookaheadMaxVelocityYUp;
        float minDistY = fallingNow ? _stats.LookaheadMinDistanceYDown : _stats.LookaheadMinDistanceYUp;
        float maxDistY = fallingNow ? _stats.LookaheadMaxDistanceYDown : _stats.LookaheadMaxDistanceYUp;
        AnimationCurve curveY = fallingNow ? _stats.LookaheadCurveYDown : _stats.LookaheadCurveYUp;

        // Deactivation is evaluated first so that activation can immediately override it
        // on the same frame (e.g. player exits deadzone while already at threshold speed).

        // Horizontal deactivate: velocity too low OR direction reversed
        if (Mathf.Abs(velocity.x) < _stats.LookaheadMinVelocityX || directionChangedX)
            _lookaheadActiveX = false;

        // Horizontal activate: follow point exits deadzone AND player is grounded.
        // If already active when the player goes airborne, it stays active in the air.
        if (followPointExitsDeadzoneX && !isAirborne) {
            _lookaheadActiveX = true;
            _lookaheadDirectionX = Mathf.Sign(velocity.x);
        }

        // Vertical: always off when grounded. Grounding gravity gives a small constant
        // downward velocity even while standing, so we guard with isGrounded before
        // any threshold check to prevent a false activation on flat ground.
        if (isGrounded) {
            _lookaheadActiveY = false;
            // Also zero the offset instantly so landing settle runs on the clean follow point
            // rather than fighting against a residual downward lookahead offset.
            _currentLookaheadOffsetY = 0f;
        } else {
            // Vertical deactivate: velocity too low for current direction OR direction reversed
            if (Mathf.Abs(velocity.y) < minVelY || directionChangedY)
                _lookaheadActiveY = false;

            // Vertical activate: follow point exits deadzone while airborne
            if (followPointExitsDeadzoneY) {
                _lookaheadActiveY = true;
                _lookaheadDirectionY = Mathf.Sign(velocity.y);
            }
        }

        float targetLookaheadX = _lookaheadActiveX ? ComputeLookaheadOffset(
            velocity.x,
            _stats.LookaheadMinVelocityX, _stats.LookaheadMaxVelocityX,
            _stats.LookaheadMinDistanceX, _stats.LookaheadMaxDistanceX,
            _stats.LookaheadCurveX) : 0f;

        bool retractingX = IsRetracting(_currentLookaheadOffsetX, targetLookaheadX);
        float dampX = retractingX ? _stats.LookaheadRetractDampingX : _stats.LookaheadDampingX;
        _currentLookaheadOffsetX = Mathf.Lerp(_currentLookaheadOffsetX, targetLookaheadX,
                                               SmoothFactor(dampX, dt));

        float targetLookaheadY = _lookaheadActiveY ? ComputeLookaheadOffset(
            velocity.y, minVelY, maxVelY, minDistY, maxDistY, curveY) : 0f;

        bool retractingY = IsRetracting(_currentLookaheadOffsetY, targetLookaheadY);
        float dampY = retractingY ? _stats.LookaheadRetractDampingY : _stats.LookaheadDampingY;
        _currentLookaheadOffsetY = Mathf.Lerp(_currentLookaheadOffsetY, targetLookaheadY,
                                               SmoothFactor(dampY, dt));

        _debugLookaheadTarget = new Vector2(
            followPoint.x + _currentLookaheadOffsetX,
            followPoint.y + _currentLookaheadOffsetY);

        // -- 2. Jump vertical lock --------------------------------------------
        // isAirborne is declared above in section 1 (lookahead)

        if (isAirborne && !_wasAirborne) {
            _jumpLockedCameraY = _currentCameraPosition.y;
            _isVerticallyLocked = true;
        }

        if (_isVerticallyLocked) {
            float fy = followPoint.y;
            if (fy > _jumpLockedCameraY + _stats.JumpVerticalLockBoundsTop ||
                fy < _jumpLockedCameraY - _stats.JumpVerticalLockBoundsBottom) {
                _isVerticallyLocked = false;
            }
        }

        if (isGrounded && _wasAirborne) {
            _isVerticallyLocked = false;
        }

        // _wasAirborne is updated after justLanded is consumed in section 6.

        // -- 3. Desired camera target -----------------------------------------
        float desiredX = followPoint.x + _currentLookaheadOffsetX;
        float desiredY = _isVerticallyLocked
            ? _jumpLockedCameraY
            : followPoint.y + _currentLookaheadOffsetY;

        // -- 4. Crouch mouse look ---------------------------------------------
        //
        // While the player holds the crouch input, the camera target shifts
        // toward the midpoint between the follow point and the mouse cursor.
        // Weight and damping are tuned in the SO.
        // This runs before sphere of influence so influence objects can still
        // nudge the final position on top of the mouse look offset.
        bool crouchHeld = _inputManager != null &&
                          _inputManager._PlayerCrouchAction != null &&
                          _inputManager._PlayerCrouchAction.IsPressed();

        if (crouchHeld && _camera != null) {
            Vector3 mouseScreen = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            mouseScreen.z = Mathf.Abs(transform.position.z - followPoint.x); // depth for perspective
            Vector3 mouseWorld = _camera.ScreenToWorldPoint(mouseScreen);
            Vector2 midpoint = Vector2.Lerp(followPoint, mouseWorld, _stats.MouseLookWeight);
            Vector2 targetMouseOffset = midpoint - followPoint;
            _currentMouseLookOffset = Vector2.Lerp(
                _currentMouseLookOffset, targetMouseOffset,
                SmoothFactor(_stats.MouseLookDamping, dt));
        } else {
            // Retract smoothly when crouch is released
            _currentMouseLookOffset = Vector2.Lerp(
                _currentMouseLookOffset, Vector2.zero,
                SmoothFactor(_stats.MouseLookRetractDamping, dt));
        }

        desiredX += _currentMouseLookOffset.x;
        desiredY += _currentMouseLookOffset.y;

        // -- 5. Sphere of influence -------------------------------------------
        Vector2 influenceOffset = ComputeInfluenceOffset(followPoint);
        _currentInfluenceOffset = Vector2.Lerp(
            _currentInfluenceOffset, influenceOffset,
            SmoothFactor(_stats.InfluenceDamping, dt));

        desiredX += _currentInfluenceOffset.x;
        desiredY += _currentInfluenceOffset.y;

        // -- 6. Deadzone gate -------------------------------------------------
        // Camera only starts chasing once desired position exits the deadzone box.
        // Exception: on the landing frame, the Y deadzone is bypassed entirely so
        // the camera immediately starts settling to the new ground height.
        // Without this, the camera sits still if followPoint.y happens to be inside
        // the deadzone after the vertical lookahead offset is cleared.
        bool justLanded = isGrounded && _wasAirborne;

        float targetX = _currentCameraPosition.x;
        float targetY = _currentCameraPosition.y;

        if (!_stats.LockHorizontal && (desiredX < _deadzoneMin.x || desiredX > _deadzoneMax.x))
            targetX = desiredX;

        if (!_stats.LockVertical) {
            if (justLanded || desiredY < _deadzoneMin.y || desiredY > _deadzoneMax.y)
                targetY = desiredY;
        }

        // -- 7. Smooth follow -------------------------------------------------
        float smoothX = SmoothFactor(_stats.DampingX, dt);
        float smoothY = SmoothFactor(justLanded ? _stats.LandingSettleDamping : _stats.DampingY, dt);

        _currentCameraPosition.x = Mathf.Lerp(_currentCameraPosition.x, targetX, smoothX);
        _currentCameraPosition.y = Mathf.Lerp(_currentCameraPosition.y, targetY, smoothY);

        // Update _wasAirborne here so justLanded correctly reads the previous frame's value.
        _wasAirborne = isAirborne;

        // -- 8. Camera bounds clamp -------------------------------------------
        if (_stats.UseCameraBounds) {
            _currentCameraPosition.x = Mathf.Clamp(_currentCameraPosition.x,
                _stats.WorldBounds.min.x + _camHalfWidth,
                _stats.WorldBounds.max.x - _camHalfWidth);
            _currentCameraPosition.y = Mathf.Clamp(_currentCameraPosition.y,
                _stats.WorldBounds.min.y + _camHalfHeight,
                _stats.WorldBounds.max.y - _camHalfHeight);
        }

        // -- 9. Re-centre deadzone on new camera position --------------------
        UpdateDeadzoneBounds();

        // -- 10. Trauma screen shake ------------------------------------------
        _trauma = Mathf.MoveTowards(_trauma, 0f, _stats.TraumaDecayRate * dt);

        Vector3 finalPosition = _currentCameraPosition;
        float finalRotZ = 0f;

        if (_trauma > 0f) {
            float intensity = _trauma * _trauma; // quadratic: fast start, smooth tail

            float nx = Mathf.PerlinNoise(_shakeTimeX, 0f) * 2f - 1f;
            float ny = Mathf.PerlinNoise(_shakeTimeY, 0f) * 2f - 1f;
            float nr = Mathf.PerlinNoise(_shakeTimeRot, 0f) * 2f - 1f;

            finalPosition.x += nx * _stats.ShakeMaxOffset * intensity;
            finalPosition.y += ny * _stats.ShakeMaxOffset * intensity;
            finalRotZ = nr * _stats.ShakeMaxRotation * intensity;

            _shakeTimeX += dt * _stats.ShakeFrequency;
            _shakeTimeY += dt * _stats.ShakeFrequency;
            _shakeTimeRot += dt * _stats.ShakeFrequency;
        }

        // -- 11. Apply --------------------------------------------------------
        transform.position = new Vector3(finalPosition.x, finalPosition.y, transform.position.z);
        transform.rotation = Quaternion.Euler(0f, 0f, finalRotZ);
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Lookahead Helpers
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Maps a signed velocity component to a signed world-space lookahead offset.
    ///
    ///   |velocity| below minVelocity  ->  0            (dead band, no lead)
    ///   |velocity| above maxVelocity  ->  maxDistance  (clamped)
    ///   in between                    ->  AnimationCurve maps normalised speed
    ///                                     to a value lerped between min/maxDistance
    ///
    /// The sign of velocity determines the direction of the lead.
    /// </summary>
    private float ComputeLookaheadOffset(
        float velocity,
        float minVelocity, float maxVelocity,
        float minDistance, float maxDistance,
        AnimationCurve curve) {

        float speed = Mathf.Abs(velocity);
        if (speed < minVelocity) return 0f;

        float range = maxVelocity - minVelocity;
        float t = range > 0f ? Mathf.Clamp01((speed - minVelocity) / range) : 1f;
        float curveT = curve != null ? curve.Evaluate(t) : t;
        float distance = Mathf.Lerp(minDistance, maxDistance, curveT);

        return Mathf.Sign(velocity) * distance;
    }

    /// <summary>
    /// True when the current offset is shrinking toward zero or has flipped direction,
    /// so the faster retract damping value should be used.
    /// </summary>
    private bool IsRetracting(float current, float target) {
        return Mathf.Abs(target) < Mathf.Abs(current) ||
               (current != 0f && Mathf.Sign(target) != Mathf.Sign(current));
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Sphere of Influence
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Call each frame to register a positional pull toward worldPosition.
    /// key    -- any unique object (use 'this') to identify the source.
    /// weight -- 0 to 1 pull strength, capped globally by InfluenceMaxWeight in the SO.
    /// Call UnregisterInfluence when the effect should stop.
    /// </summary>
    public void RegisterInfluence(object key, Vector2 worldPosition, float weight) {
        weight = Mathf.Clamp01(weight);
        for (int i = 0; i < _activeInfluences.Count; i++) {
            if (_activeInfluences[i].Key == key) {
                _activeInfluences[i] = new CameraInfluenceData(key, worldPosition, weight);
                return;
            }
        }
        _activeInfluences.Add(new CameraInfluenceData(key, worldPosition, weight));
    }

    public void UnregisterInfluence(object key) {
        for (int i = _activeInfluences.Count - 1; i >= 0; i--) {
            if (_activeInfluences[i].Key == key) {
                _activeInfluences.RemoveAt(i);
                return;
            }
        }
    }

    private Vector2 ComputeInfluenceOffset(Vector2 followPoint) {
        if (_activeInfluences.Count == 0) return Vector2.zero;

        float totalWeight = 0f;
        Vector2 weightedSum = Vector2.zero;

        foreach (var inf in _activeInfluences) {
            float w = Mathf.Clamp01(inf.Weight) * _stats.InfluenceMaxWeight;
            weightedSum += (inf.WorldPosition - followPoint) * w;
            totalWeight += w;
        }

        if (totalWeight <= 0f) return Vector2.zero;

        return totalWeight > _stats.InfluenceMaxWeight
            ? weightedSum / totalWeight * _stats.InfluenceMaxWeight
            : weightedSum;
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Screen Shake -- Public API
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Add trauma (0 to 1). Accumulates and decays automatically each frame.
    /// Shake intensity = trauma^2 for a natural quadratic falloff.
    /// Suggested values: small hit 0.2 / large hit 0.5 / explosion 0.8
    /// </summary>
    public void AddTrauma(float amount) => _trauma = Mathf.Clamp01(_trauma + amount);

    /// <summary>Immediately cut all screen shake.</summary>
    public void ClearTrauma() => _trauma = 0f;

    public float GetTrauma() => _trauma;

    #endregion

    ////////////////////////////////////////////////////////////
    #region Snap -- Public API
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Instantly reposition the camera onto the follow point with no damping.
    /// Call this after a player teleport so the camera does not slide across the world.
    /// </summary>
    public void SnapToFollowPoint() {
        _currentCameraPosition = new Vector3(
            _cameraFollowPlayerTransform.position.x,
            _cameraFollowPlayerTransform.position.y,
            transform.position.z);

        transform.position = _currentCameraPosition;
        _currentLookaheadOffsetX = 0f;
        _currentLookaheadOffsetY = 0f;
        _lookaheadActiveX = false;
        _lookaheadActiveY = false;
        _lookaheadDirectionX = 0f;
        _lookaheadDirectionY = 0f;
        _currentInfluenceOffset = Vector2.zero;
        _jumpLockedCameraY = _currentCameraPosition.y;
        _isVerticallyLocked = false;

        UpdateDeadzoneBounds();
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Helpers
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Converts a damping value [0, 1] into a framerate-independent lerp factor
    /// using exponential decay: 1 - e^(-rate * dt)
    ///   0 -> snap instantly
    ///   1 -> never move
    /// </summary>
    private float SmoothFactor(float damping, float dt) {
        if (damping <= 0f) return 1f;
        if (damping >= 1f) return 0f;
        float rate = -Mathf.Log(damping) * 10f;
        return 1f - Mathf.Exp(-rate * dt);
    }

    private void UpdateDeadzoneBounds() {
        Vector2 half = _stats != null ? _stats.DeadzoneSize * 0.5f : Vector2.one * 0.5f;
        _deadzoneMin = new Vector2(_currentCameraPosition.x - half.x, _currentCameraPosition.y - half.y);
        _deadzoneMax = new Vector2(_currentCameraPosition.x + half.x, _currentCameraPosition.y + half.y);
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Gizmos
    ////////////////////////////////////////////////////////////

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        // _stats is a serialized SO reference -- available in Edit mode and Play mode alike.
        // All gizmo flags and values are read directly from it, so the booleans are always
        // live and never depend on runtime initialization having run first.
        if (_stats == null) return;

        Vector3 camPos = transform.position;

        // -- Centre crosshair -------------------------------------------------
        if (_stats.ShowCenterCrosshair) {
            float half = _stats.CrosshairSize;
            Gizmos.color = new Color(1f, 1f, 1f, 0.9f);
            Gizmos.DrawLine(camPos + Vector3.left * half, camPos + Vector3.right * half);
            Gizmos.DrawLine(camPos + Vector3.down * half, camPos + Vector3.up * half);
            Gizmos.DrawWireCube(camPos, Vector3.one * 0.06f);
        }

        // -- Deadzone box -----------------------------------------------------
        if (_stats.ShowDeadzone) {
            Vector3 size = new Vector3(_stats.DeadzoneSize.x, _stats.DeadzoneSize.y, 0f);
            Gizmos.color = new Color(0f, 1f, 0.4f, 0.6f);
            Gizmos.DrawWireCube(camPos, size);
            Gizmos.color = new Color(0f, 1f, 0.4f, 0.06f);
            Gizmos.DrawCube(camPos, size);
        }

        // -- Lookahead target -------------------------------------------------
        if (_stats.ShowLookaheadPoint) {
            Vector3 lt = new Vector3(_debugLookaheadTarget.x, _debugLookaheadTarget.y, camPos.z);
            float d = 0.18f;

            Gizmos.color = new Color(1f, 0.8f, 0f, 0.95f);
            Gizmos.DrawLine(lt + Vector3.up * d, lt + Vector3.right * d);
            Gizmos.DrawLine(lt + Vector3.right * d, lt + Vector3.down * d);
            Gizmos.DrawLine(lt + Vector3.down * d, lt + Vector3.left * d);
            Gizmos.DrawLine(lt + Vector3.left * d, lt + Vector3.up * d);

            Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
            Gizmos.DrawLine(camPos, lt);
        }

        // -- Jump vertical lock band ------------------------------------------
        if (_isVerticallyLocked) {
            float midY = _jumpLockedCameraY +
                           (_stats.JumpVerticalLockBoundsTop - _stats.JumpVerticalLockBoundsBottom) * 0.5f;
            float height = _stats.JumpVerticalLockBoundsTop + _stats.JumpVerticalLockBoundsBottom;
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireCube(new Vector3(camPos.x, midY, camPos.z), new Vector3(6f, height, 0f));
        }

        // -- World camera bounds ----------------------------------------------
        if (_stats.UseCameraBounds) {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
            Gizmos.DrawWireCube(_stats.WorldBounds.center, _stats.WorldBounds.size);
        }
    }
#endif

    #endregion
}

////////////////////////////////////////////////////////////
#region Supporting Types
////////////////////////////////////////////////////////////

/// <summary>
/// Snapshot of a single sphere-of-influence source for one frame.
/// Influence objects create these by calling CameraHandler.RegisterInfluence().
/// </summary>
public readonly struct CameraInfluenceData {
    public readonly object Key;
    public readonly Vector2 WorldPosition;
    public readonly float Weight;

    public CameraInfluenceData(object key, Vector2 worldPosition, float weight) {
        Key = key;
        WorldPosition = worldPosition;
        Weight = weight;
    }
}

#endregion