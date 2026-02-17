using EditorAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class JLPlayerTest : Singleton<JLPlayerTest>, IBarnakTarget
{
    [Header("Movement")]
    [SerializeField] float groundAcceleration = 10;
    [SerializeField] float airAcceleration = 10;
    [SerializeField] float groundFriction = 10;
    [SerializeField] float airFriction = 10;
    [SerializeField] float jumpSpeed = 1;

    [Header("Input")]
    [SerializeField] Key leftKey = Key.A;
    [SerializeField] Key rightKey = Key.D;
    [SerializeField] Key jumpKey = Key.Space;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] Vector2 groundCheckSize = new Vector2(0.95f, 0.1f);
    [SerializeField] LayerMask groundLayer;
    [SerializeField, ReadOnly] bool isGrounded = false;

    [Header("Barnak")]
    [SerializeField, ReadOnly] Barnak barnakCaught = null;
    [SerializeField]float barnakTargetRadius = 0.6f;
    [SerializeField] int hitsToRelease = 5;
    [SerializeField] Shake shake;
    [SerializeField] float shakeAmplitude = 1;
    int hitsCount = 0;

    Rigidbody2D rb;

    public float BarnakTargetRadius => barnakTargetRadius;

    void OnDrawGizmosSelected()
    {
        DrawGroundCheck();
        DrawBarnakTargetRadius();
    }

    void DrawGroundCheck()
    {
        if (groundCheck == null) 
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

    void DrawBarnakTargetRadius()
    {
        Gizmos.color = Color.yellow;
        GizmosExtension.DrawCircle(transform.position, barnakTargetRadius);
    }

    void Reset()
    {
        groundCheck = transform;
    }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckJump();
        CheckHitBarnak();
    }

    void FixedUpdate()
    {
        UpdateIsGrounded();
        ApplyAcceleration();
        ApplyFriction();
    }

    void UpdateIsGrounded()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    void ApplyAcceleration()
    {
        if (barnakCaught)
            return;

        float xInput = 0;
        if (Keyboard.current[leftKey].isPressed)  xInput--;
        if (Keyboard.current[rightKey].isPressed) xInput++;

        if (xInput == 0)
            return;

        float acceleration = isGrounded ? groundAcceleration : airAcceleration;
        rb.linearVelocityX += xInput * acceleration * Time.fixedDeltaTime;
    }

    void ApplyFriction()
    {        
        if (barnakCaught)
            return;

        float friction = isGrounded ? groundFriction : airFriction;
        rb.linearVelocityX *= 1 - friction * Time.fixedDeltaTime;
    }

    void CheckJump()
    {
        if (isGrounded &&
            Keyboard.current[jumpKey].wasPressedThisFrame &&
            !barnakCaught)
            Jump();
    }

    void Jump()
    {
        rb.linearVelocityY = jumpSpeed;
    }

    void CheckHitBarnak()
    {
        if (barnakCaught &&
            Keyboard.current[jumpKey].wasPressedThisFrame)
            HitBarnakToRelease();
    }

    void HitBarnakToRelease()
    {
        if (!barnakCaught)
            return;

        hitsCount++;

        if (hitsCount >= hitsToRelease)
            barnakCaught.ReleaseTarget();

        else if (shake)
            shake.AddAmplitude += shakeAmplitude;
    }

    public void OnBarnakCaught(Barnak barnak)
    {
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
        barnakCaught = barnak;
        hitsCount = 0;
    }

    public void OnBarnakEat(Barnak barnak)
    {
        rb.simulated = true;
        barnakCaught = null;
        hitsCount = 0;
    }

    public void OnBarnakRelease(Barnak barnak)
    {
        rb.simulated = true;
        barnakCaught = null;
        hitsCount = 0;
    }
}
