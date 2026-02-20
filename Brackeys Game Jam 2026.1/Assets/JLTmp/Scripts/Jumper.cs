using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Jumper : MonoBehaviour, IBarnakTarget
{
    [Header("Animation")]
    [SerializeField, ReadOnly] bool lookRight = true;
    [SerializeField] Transform lookRightTransform;
    [SerializeField] Animator squashAnimator;

    [Header("Sprite")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite isGroundedSprite;
    [SerializeField] Sprite isInAirSprite;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] Vector2 groundCheckSize = new Vector2(0.95f, 0.1f);
    [SerializeField] LayerMask groundLayer;
    [SerializeField, ReadOnly] protected bool isGrounded = false;

    [Header("Jump")]
    [SerializeField] protected float jumpSpeed = 8;
    [SerializeField, Range(0, 90)] protected float jumpMaxAngle = 30;
    [SerializeField] protected Vector2 dtJumpRange = new Vector2(0, 3);
    protected bool isGroundedLocked = false;
    
    [Header("Barnak")]
    [SerializeField] float barnakTargetRadius = 0.6f;
    Barnak barnakCaught = null;

    [Header("Sounds")]
    [SerializeField] protected string notifyAkOnJump = "Jumper_Jump";
    [SerializeField] protected string notifyAkOnLand = "Jumper_Land";


    protected Rigidbody2D rb;

    public float BarnakTargetRadius => barnakTargetRadius;

    protected virtual void OnDrawGizmosSelected()
    {
        DrawGroundCheck();
        DrawJumpMaxAngle();
        DrawBarnakTargetRadius();
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

    void DrawBarnakTargetRadius()
    {
        Gizmos.color = Color.yellow;
        GizmosExtension.DrawCircle(transform.position, barnakTargetRadius);
    }


    protected virtual void Reset()
    {
        groundCheck = transform;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SpriteMatchIsGrounded();
    }

    void OnDisable()
    {
        if (barnakCaught)
        {
            barnakCaught.ReleaseTarget();
            barnakCaught = null;
        }

        CancelInvoke("UnlockIsGrounded");
        UnlockIsGrounded();
        SetIsGrounded(false);
    }

    protected virtual void FixedUpdate()
    {
        UpdateIsGrounded();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (isGrounded)
            return;

        print("Jumper.OnTriggerEnter2D : Hit...");
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

        if (gameObject.activeInHierarchy)
            squashAnimator.Play("Squash", 0, 0);
        
        SpriteMatchIsGrounded();
        
        if (isGrounded)
        {
            Invoke("Jump", dtJumpRange.RandomInRange());
            
            if (notifyAkOnLand != "")
                AkUnitySoundEngine.PostEvent(notifyAkOnLand, gameObject);
        } 

        else CancelInvoke("Jump");
    }

    protected void LockIsGrounded() => isGroundedLocked = true;
    protected void UnlockIsGrounded() => isGroundedLocked = false;

    protected void SpriteMatchIsGrounded() => spriteRenderer.sprite = isGrounded ? isGroundedSprite : isInAirSprite;

    protected virtual void Jump()
    {
        float angle = Random.Range(-jumpMaxAngle, jumpMaxAngle);
        Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
        rb.linearVelocity = direction * jumpSpeed;

        if (notifyAkOnJump != "")
            AkUnitySoundEngine.PostEvent(notifyAkOnJump, gameObject);

        SetLookRight(rb.linearVelocity.x > 0);
        SetIsGrounded(false);
        LockIsGrounded();
        Invoke("UnlockIsGrounded", 0.1f);
    }

    protected void SetLookRight(bool lookRight)
    {
        if (this.lookRight == lookRight)
            return;

        this.lookRight = lookRight;
        lookRightTransform.transform.SetXScale(lookRight ? 1 : -1);
        
        squashAnimator.Play("Squash", 0, 0);
    }

    public void OnBarnakCaught(Barnak barnak)
    {
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
        barnakCaught = barnak;
    }

    public void OnBarnakRelease(Barnak barnak)
    {
        rb.simulated = true;
        barnakCaught = null;
    }
    
    public void OnBarnakEat(Barnak barnak, GroundedBarnak groundedBarnak)
    {
        Destroy(gameObject);
    }
}
