using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour {

    [Header("Debug Visualization")]
    [SerializeField] private bool _debugGroundCheck = false;
    [SerializeField] private bool _debugHeadCheck = false;
    [SerializeField] private bool _debugWallCheck = false;
    [SerializeField] private bool _debugLedgeCheck = false;

    [Header("Collision Points")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Transform _wallCheckPoint;
    [SerializeField] private Transform _headCheckPoint;
    [SerializeField] private Transform _ledgeCheckPoint;

    [Header("Collider References")]
    [SerializeField] private CapsuleCollider2D _bodyCollider;

    [Header("Collision Detection")]
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private float GroundDetectionRayLength = 0.02f;
    [SerializeField] private float HeadDetectionRayLength = 0.02f;
    [SerializeField] private float HeadWidth = 0.75f;
    [SerializeField] private float WallDetectionRayLength = 0.02f;
    [Range(0.01f, 2f)][SerializeField] private float WallDetectionRayHeightMultiplier = 0.9f;
    [Range(0.01f, 1f)][SerializeField] private float GroundBodyWidthMultiplier = 0.9f;

    [Header("Ledge Forgiveness")]
    [SerializeField] private bool _enableLedgeForgiveness = true;
    [SerializeField] private float _ledgeSnapThreshold = 0.4f;
    [SerializeField] private float _ledgeRayLength = 1.0f;

    private PlayerStatsHandler _stats;
    private PlayerBlackboardHandler _blackboard;
    private PlayerHandler _handler;

    private Vector2 _groundCheckSize;
    private Vector2 _wallCheckSize;
    private Vector2 _headCheckSize;

    private bool _wasGroundedLastFrame;
    private bool _isledgeSnapOnCooldown;

    public void Initialize(PlayerHandler handler, PlayerStatsHandler stats, PlayerBlackboardHandler blackboard) {
        _handler = handler;
        _stats = stats;
        _blackboard = blackboard;
        CalculateCheckSizes();
    }

    private void CalculateCheckSizes() {
        if (_bodyCollider == null) return;

        _groundCheckSize = new Vector2(
            _bodyCollider.size.x * GroundBodyWidthMultiplier,
            GroundDetectionRayLength
        );

        _wallCheckSize = new Vector2(
            WallDetectionRayLength,
            _bodyCollider.size.y * WallDetectionRayHeightMultiplier
        );

        _headCheckSize = new Vector2(
            HeadWidth,
            HeadDetectionRayLength
        );
    }

    // Flip the local scale of this GameObject so all child check points mirror automatically.
    // Author all check points on the RIGHT side in the inspector (positive local X).
    // When the player faces left this flips the whole handler so they land on the left side.
    private void UpdateFacingScale() {
        float targetScaleX = _blackboard.IsFacingRight ? 1f : -1f;
        if (transform.localScale.x != targetScaleX) {
            transform.localScale = new Vector3(targetScaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    public void UpdateCollisionChecks() {
        _wasGroundedLastFrame = _blackboard.IsGrounded;

        // Flip first so all child transforms are in the right world position before casting
        UpdateFacingScale();

        CheckGround();
        CheckWall();
        CheckHead();

        if (_enableLedgeForgiveness) {
            CheckLedgeForgiveness();
        }

        if (_blackboard.IsGrounded && !_wasGroundedLastFrame) {
            _blackboard.OnLanded();
        } else if (!_blackboard.IsGrounded && _wasGroundedLastFrame) {
            _blackboard.OnLeftGround();
        }
    }

    private void CheckGround() {
        Vector2 origin = _groundCheckPoint != null
            ? (Vector2)_groundCheckPoint.position
            : (Vector2)transform.position - new Vector2(0f, _bodyCollider.size.y * 0.5f);

        _blackboard.IsGrounded = Physics2D.BoxCast(
            origin,
            _groundCheckSize,
            0f,
            Vector2.down,
            GroundDetectionRayLength,
            GroundLayer
        );
    }

    private void CheckWall() {
        // The wall check point is already on the correct side because UpdateFacingScale()
        // flipped the handler's local X before this runs.
        Vector2 origin = _wallCheckPoint != null
            ? (Vector2)_wallCheckPoint.position
            : (Vector2)transform.position;

        Vector2 facingDir = _blackboard.IsFacingRight ? Vector2.right : Vector2.left;
        int facingSign = _blackboard.IsFacingRight ? 1 : -1;

        bool hitWall = Physics2D.BoxCast(
            origin,
            _wallCheckSize,
            0f,
            facingDir,
            WallDetectionRayLength,
            GroundLayer
        );

        _blackboard.IsAgainstWall = hitWall;

        if (hitWall) {
            _blackboard.WallDirection = facingSign;
            _blackboard.OnWallTouch();
        } else {
            _blackboard.WallDirection = 0;
        }
    }

    private void CheckHead() {
        _blackboard.IsHeadBlocked = Physics2D.BoxCast(
            (Vector2)_headCheckPoint.position,
            _headCheckSize,
            0f,
            Vector2.up,
            HeadDetectionRayLength,
            GroundLayer
        );
    }

    private void CheckLedgeForgiveness() {
        if (_blackboard.IsGrounded || _blackboard.IsJumping || _blackboard.Velocity.y > 0f || _blackboard.MoveInput.x < _stats.MoveThreshold || _isledgeSnapOnCooldown) return;

        float facingSign = _blackboard.IsFacingRight ? 1f : -1f;

        RaycastHit2D hit = Physics2D.Raycast(
            (Vector2)_ledgeCheckPoint.position,
            Vector2.down,
            _ledgeRayLength,
            GroundLayer
        );

        if (hit.collider == null) return;

        float ledgeTopY = hit.point.y;
        float playerFeetY = transform.position.y;
        float heightDiff = ledgeTopY - playerFeetY;

        if (heightDiff > 0f && heightDiff < _ledgeSnapThreshold) {
            Vector3 snapPos = transform.position;
            snapPos.y = ledgeTopY;
            snapPos.x = hit.point.x - facingSign * _ledgeCheckPoint.localPosition.x * 0.5f;
            _handler.Teleport(snapPos);
            _blackboard.Velocity.y = 0f;

            if (_debugLedgeCheck) {
                Debug.Log($"[LedgeForgiveness] Snapped! Feet were {heightDiff:F3}m below ledge.");
            }

            StartLedgeSnapCooldown();
        }
    }

    private async void StartLedgeSnapCooldown() {
        _isledgeSnapOnCooldown = true;
        while (!_blackboard.IsGrounded || _blackboard.IsAgainstWall) {
            await Awaitable.FixedUpdateAsync();
        }
        _isledgeSnapOnCooldown = false;
    }

    public bool IsLedgeAhead(float checkDistance = 1f) {
        float facingSign = _blackboard.IsFacingRight ? 1f : -1f;
        Vector2 origin = (Vector2)transform.position + new Vector2(facingSign * checkDistance, -0.5f);

        return !Physics2D.Raycast(
            origin,
            Vector2.down,
            GroundDetectionRayLength * 2f,
            GroundLayer
        );
    }

    private void OnDrawGizmos() {
        if (_bodyCollider == null) return;

        if (_debugGroundCheck) {
            bool grounded = Application.isPlaying && _blackboard != null && _blackboard.IsGrounded;
            Gizmos.color = grounded ? Color.green : Color.red;

            Vector2 pos = _groundCheckPoint != null
                ? (Vector2)_groundCheckPoint.position
                : (Vector2)transform.position - new Vector2(0f, _bodyCollider.size.y * 0.5f);

            Gizmos.DrawWireCube(pos, _groundCheckSize);
        }

        if (_debugWallCheck) {
            bool againstWall = Application.isPlaying && _blackboard != null && _blackboard.IsAgainstWall;
            Gizmos.color = againstWall ? Color.green : Color.red;

            Vector2 wallOrigin = _wallCheckPoint != null
                ? (Vector2)_wallCheckPoint.position
                : (Vector2)transform.position;

            Gizmos.DrawWireCube(wallOrigin, _wallCheckSize);
        }

        if (_debugHeadCheck) {
            bool headBlocked = Application.isPlaying && _blackboard != null && _blackboard.IsHeadBlocked;
            Gizmos.color = headBlocked ? Color.green : Color.red;

            Vector2 pos = _headCheckPoint != null
                ? (Vector2)_headCheckPoint.position
                : (Vector2)transform.position + new Vector2(0f, _bodyCollider.size.y * 0.5f);

            Gizmos.DrawWireCube(pos, _headCheckSize);
        }

        if (_debugLedgeCheck && _enableLedgeForgiveness) {
            Gizmos.color = Color.yellow;

            Vector2 ledgeOrigin;
            if (_ledgeCheckPoint != null) {
                ledgeOrigin = (Vector2)_ledgeCheckPoint.position;
            } else {
                bool facingRight = !Application.isPlaying || _blackboard == null || _blackboard.IsFacingRight;
                float facingSign = facingRight ? 1f : -1f;
                ledgeOrigin = (Vector2)transform.position + new Vector2(facingSign * (_bodyCollider.size.x * 0.5f + 0.1f), 0f);
            }

            Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * _ledgeRayLength);
            Gizmos.DrawWireSphere(ledgeOrigin, 0.04f);
        }
    }
}