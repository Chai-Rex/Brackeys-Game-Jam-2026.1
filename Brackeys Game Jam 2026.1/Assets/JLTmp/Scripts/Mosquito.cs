using System.Linq;
using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Mosquito : MonoBehaviour
{
    [SerializeField] float randomAcceleration = 5;
    [SerializeField] float noiseScale = 1;
    Vector2 noiseOffset;
    [SerializeField] float friction = 1;

    [Space(15)]
    [SerializeField, ReadOnly] MosquitoTarget target = null;
    [SerializeField] float viewTargetRadius = 3;
    [SerializeField] float accelerationToTarget = 1;
    [SerializeField] float targetFriction = 2;
    [SerializeField, ReadOnly] bool isLanded = false;
    [SerializeField] float landMaxSpeed = 0.1f;
    [SerializeField, ReadOnly] bool takingOff = false;
    [SerializeField] Vector2 takingOffTimeRange = new (0.8f, 1.2f);
    [SerializeField] Vector2 takeOffSpeedRange = new(1.5f, 2f);

    [Space(15)]
    [SerializeField] float groundRepulsion = 1f;
    [SerializeField] float groundRaycastDistance = 0.5f;
    [SerializeField] LayerMask groundLayer;

    [Space(15)]
    [SerializeField] GameObject spritesParent;
    [SerializeField] GameObject activeOnFlying;
    [SerializeField] GameObject activeOnLanded;
    [SerializeField] string squashAnimName = "MosquitoSquash";
    bool lookRight = true;

    Rigidbody2D rb;
    Animator animator;

    public bool IsLanded => isLanded;
    public bool IsFlying => !isLanded;

    void OnDrawGizmosSelected()
    {
        DrawViewTargetRadius();
        DrawGroundRaycast();
    }

    void DrawViewTargetRadius()
    {
        Gizmos.color = Color.yellow;
        GizmosExtension.DrawCircle(transform.position, Vector3.forward, viewTargetRadius);
    }

    void DrawGroundRaycast()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundRaycastDistance);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        noiseOffset = RandomExtension.RandomVector2() * 1000;
        activeOnFlying.SetActive(!isLanded);
        activeOnLanded.SetActive(isLanded);
    }

    void FixedUpdate()
    {
        ApplyRandomAcceleration();
        GroundRepulsion();
        UpdateTarget();
        ApplyTargetAcceleration();
        ApplyTargetFriction();
        UpdateIsLanded();
        ApplyFriction();
        UpdateLook();
    }

    Vector2 ApplyRandomAcceleration()
    {
        if (target && 
            (target.transform.position - transform.position).sqrMagnitude < target.SqrRadius)
            return Vector2.zero;

        Vector2 noiseValue = new(Mathf.PerlinNoise1D(noiseOffset.x + Time.time * noiseScale) * 2f - 0.9305f, 
                                 Mathf.PerlinNoise1D(noiseOffset.y + Time.time * noiseScale) * 2f - 0.9305f);

        Vector2 acceleration = randomAcceleration * noiseValue;
        rb.linearVelocity += acceleration * Time.fixedDeltaTime;
        return acceleration;
    }

    void ApplyFriction()
    {
        rb.linearVelocity *= 1 - friction * Time.fixedDeltaTime;
    }

    Vector2 GroundRepulsion()
    {
        if (target ||
            !Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayer))
            return Vector2.zero;

        Vector2 acceleration = groundRepulsion * Vector2.up;
        rb.linearVelocity += acceleration * Time.fixedDeltaTime;
        return acceleration;
    }

    void UpdateTarget()
    {
        if (takingOff)
            return;

        float sqrRadius = viewTargetRadius * viewTargetRadius;

        if (MosquitoTarget.RbTargetsCount > 0)
        {
            MosquitoTarget closestTarget = MosquitoTarget.RbTargets.OrderBy((t) => (transform.position - t.transform.position).sqrMagnitude).First();
            float sqrDist = (transform.position - closestTarget.transform.position).sqrMagnitude;

            if (sqrDist <= sqrRadius)
            {
                SetTarget(closestTarget);
                return;
            }
        }

        if (MosquitoTarget.NoRbTargetsCount > 0)
        {
            MosquitoTarget closestTarget = MosquitoTarget.NoRbTargets.OrderBy((t) => (transform.position - t.transform.position).sqrMagnitude).First();
            float sqrDist = (transform.position - closestTarget.transform.position).sqrMagnitude;

            if (sqrDist <= sqrRadius)
            {
                SetTarget(closestTarget);
                return;
            }
        }

        SetTarget(null);
    }

    Vector2 ApplyTargetAcceleration()
    {
        if (!target)
            return Vector2.zero;

        Vector2 toTarget = target.transform.position - transform.position;

        if (toTarget.sqrMagnitude < target.SqrRadius) 
            return Vector2.zero;

        Vector2 accelerration = accelerationToTarget * toTarget.normalized;
        rb.linearVelocity += accelerration * Time.fixedDeltaTime;
        return accelerration;
    }

    void ApplyTargetFriction()
    {
        if (!target)
            return;

        Vector2 toTarget = target.transform.position - transform.position;
        float sqrDist = toTarget.sqrMagnitude;

        if (sqrDist > target.SqrRadius)
            return;

        rb.linearVelocity *= 1 - targetFriction * Time.fixedDeltaTime;
    }

    void UpdateIsLanded()
    {
        SetIsLanded(target && 
                    (target.transform.position - transform.position).sqrMagnitude < target.SqrRadius &&
                    rb.linearVelocity.sqrMagnitude < landMaxSpeed * landMaxSpeed &&
                    (!target.Rigidbody2D || target.Rigidbody2D.linearVelocity.sqrMagnitude < landMaxSpeed * landMaxSpeed));
    }

    void SetTarget(MosquitoTarget target)
    {
        if (this.target == target)
            return;

        if (isLanded)
            TakeOff();

        this.target = target;
    }

    void SetIsLanded(bool isLanded, bool calledFromTakeOff = false)
    {
        if (this.isLanded == isLanded)
            return;

        this.isLanded = isLanded;
        activeOnFlying.SetActive(!isLanded);
        activeOnLanded.SetActive(isLanded);

        animator.Play(squashAnimName, 0, 0);

        if (!isLanded &&
            !calledFromTakeOff)
            TakeOff(true);
    }

    void TakeOff(bool calledFromSetLanded = false)
    {
        if (!isLanded && 
            !calledFromSetLanded)
            return;

        Vector2 toTarget = target.transform.position - transform.position;
        
        if (toTarget == Vector2.zero)
            toTarget.y -= 0.001f;

        rb.linearVelocity = -toTarget.normalized * takeOffSpeedRange.RandomInRange();

        target = null;

        takingOff = true;
        Invoke("SetTakingOffFalse", takingOffTimeRange.RandomInRange());

        if (!calledFromSetLanded)
            SetIsLanded(false, true);
    }

    void SetTakingOffFalse() => takingOff = false;

    void SetLookRight(bool lookRight)
    {
        if (this.lookRight == lookRight)
            return;

        this.lookRight = lookRight;
        spritesParent.transform.SetXScale(lookRight ? 1 : -1);
        
        animator.Play(squashAnimName, 0, 0);
    }

    void UpdateLook()
    {
        if (takingOff)
            return;

        SetLookRight(rb.linearVelocity.x > 0);
    }
}
