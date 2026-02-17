using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Jumper : MonoBehaviour, IBarnakTarget
{
    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] Vector2 groundCheckSize = new Vector2(0.95f, 0.1f);
    [SerializeField] LayerMask groundLayer;
    [SerializeField, ReadOnly] protected bool isGrounded = false;

    [Header("Jump")]
    [SerializeField] protected float jumpSpeed = 8;
    [SerializeField, Range(0, 90)] protected float jumpMaxAngle = 30;
    [SerializeField] protected Vector2 dtJumpRange = new Vector2(0, 3);

    [Header("Barnak")]
    [SerializeField] float barnakTargetRadius = 0.6f;
    Barnak barnakCaught = null;

    protected bool isGroundedLocked = false;

    protected Rigidbody2D rb;

    public float BarnakTargetRadius => barnakTargetRadius;

    protected virtual void OnDrawGizmosSelected()
    {
        DrawGroundCheck();
        DrawJumpMaxAngle();
    }

    void DrawGroundCheck()
    {
        if (groundCheck == null) 
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

    void DrawJumpMaxAngle()
    {
        Gizmos.color = Color.green;
        GizmosExtension.DrawArc(transform.position, Vector3.up, Vector3.forward, jumpMaxAngle * 2, 2);
    }

    protected virtual void Reset()
    {
        groundCheck = transform;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        UpdateIsGrounded();
    }

    void UpdateIsGrounded()
    {
        if (isGroundedLocked)
            return;
            
        SetIsGrounded(Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer));
    }

    protected virtual void SetIsGrounded(bool isGrounded)
    {
        if (isGroundedLocked ||
            this.isGrounded == isGrounded)
            return;

        this.isGrounded = isGrounded;

        if (isGrounded) Invoke("Jump", dtJumpRange.RandomInRange());
        else            CancelInvoke("Jump");
    }


    protected void LockIsGrounded() => isGroundedLocked = true;
    protected void UnlockIsGrounded() => isGroundedLocked = false;

    protected virtual void Jump()
    {
        float angle = Random.Range(-jumpMaxAngle, jumpMaxAngle);
        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
        rb.linearVelocity = direction * jumpSpeed;

        SetIsGrounded(false);
        LockIsGrounded();
        Invoke("UnlockIsGrounded", 0.1f);
    }

    public void OnBarnakCaught(Barnak barnak)
    {
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
        CancelInvoke("Jump");
    }

    public void OnBarnakEat(Barnak barnak)
    {
        Destroy(gameObject);
    }

    public void OnBarnakRelease(Barnak barnak) {}
}
