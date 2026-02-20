using System.Collections;
using System.Linq;
using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Umbrelloid : MonoBehaviour
{
    [SerializeField] float randomAcceleration = 1.5f;
    [SerializeField] float noiseScale = 0.5f;
    Vector2 noiseOffset;
    [SerializeField] float friction = 0.5f;

    [Space(15)]
    [SerializeField] float groundRepulsion = 1f;
    [SerializeField] float groundRepulsionRadius = 1.4f;
    [SerializeField] float groundRaycastLength = 3f;
    [SerializeField] LayerMask groundLayer;

    [Space(15)]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite spriteOpened;
    [SerializeField] Sprite spriteClosed;
    [SerializeField] float rotateWithAcceleration = 1;
    [SerializeField] float rotateBackSpeed = 1;
    [SerializeField] AnimationCurve rotateBackCurve;

    [Space(15)]
    [SerializeField] Collider2D triggerOpened;
    [SerializeField] Collider2D triggerClosed;    
    [SerializeField] Collider2D colliderOpened;
    [SerializeField] Collider2D colliderClosed;
    [SerializeField] float gravity = 1;

    [Space(15)]
    [SerializeField, ReadOnly] bool opened = true;
    [SerializeField, ReadOnly] bool grounded = false;
    [SerializeField] Vector2 freeGroundTimeRange = new Vector2(2, 4);
    [SerializeField] AnimationCurve freeGroundShakeCurve;
    [SerializeField] Shake shake;
    [SerializeField, ReadOnly] bool justFreedGround = false;
    [SerializeField] float justFreedGroundTime = 1;

    [Space(15)]
    [SerializeField] float dmg = 1;

    Rigidbody2D rb;

    void OnDrawGizmosSelected()
    {
        DrawGroundRepulsion();
    }

    void DrawGroundRepulsion()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundRaycastLength);
        GizmosExtension.DrawCircle(transform.position, groundRepulsionRadius);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        triggerOpened.enabled = opened;
        triggerClosed.enabled = !opened;
        colliderOpened.enabled = opened;
        colliderClosed.enabled = !opened;
        noiseOffset = RandomExtension.RandomVector2() * 1000;
    }

    void OnDisable()
    {
        CancelInvoke("SetJustFreedGroundFalse");
        SetJustFreedGroundFalse();

        if (grounded)
        {
            shake.enabled = false;
            shake.BaseAmplitude = 0;

            SetGrounded(false);
            SetOpened(true);
        }

        else if (!opened)
        {
            StopCoroutine("RotateBack");
            spriteRenderer.transform.up = Vector2.up;
            SetOpened(true);
        }
    }

    void FixedUpdate()
    {
        if (opened)
        {
            Vector2 randomAccelerationApplied = ApplyRandomAcceleration();
            RotateWithAcceleration(randomAccelerationApplied);
            GroundRepulsion();
            ApplyFriction();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponentInParentOrInChildren(out IUmbrelloidTarget target))
                return;

        if (opened)
        {
            if (justFreedGround)
                return;
                
            SetOpened(false);
        }

        else {
            target.OnUmbrelloidHit(dmg);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (opened ||
            grounded)
            return;

        SetGrounded(true);
    }

    void SetOpened(bool opened)
    {
        if (this.opened == opened)
            return;

        this.opened = opened;
        triggerOpened.enabled = opened;
        triggerClosed.enabled = !opened;
        colliderOpened.enabled = opened;
        colliderClosed.enabled = !opened;
        spriteRenderer.sprite = opened ? spriteOpened : spriteClosed;
        rb.gravityScale = opened ? 0 : gravity;

        if (opened) StopCoroutine("RotateBack");
        else        StartCoroutine("RotateBack");
    }

    void SetGrounded(bool grounded)
    {
        if (this.grounded == grounded)
            return;

        this.grounded = grounded;
        rb.simulated = !grounded;
        
        if (grounded)
        {
            rb.linearVelocity = Vector2.zero;
            StartCoroutine(FreeGround(freeGroundTimeRange.RandomInRange()));
        }
    }

    Vector2 ApplyRandomAcceleration()
    {
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
        
        GroundData groundData = GroundDataGrid.Instance.GetData(transform.position);

        if (groundData.HasHit &&
            groundData.GroundDistance < groundRepulsionRadius)
        {
            Vector2 acceleration = groundRepulsion * groundData.OutDirection;
            rb.linearVelocity += acceleration * Time.fixedDeltaTime;
            return acceleration;
        }


        else if (Physics2D.Raycast(transform.position, Vector2.down, groundRaycastLength, groundLayer))
        {
            Vector2 acceleration = groundRepulsion * Vector2.up;
            rb.linearVelocity += acceleration * Time.fixedDeltaTime;
            return acceleration;
        }
        
        return Vector2.zero;
    }

    void RotateWithAcceleration(Vector2 acceleration) 
        => spriteRenderer.transform.SetZEuler(-acceleration.x * rotateWithAcceleration);

    IEnumerator RotateBack()
    {
        Vector2 startUp = spriteRenderer.transform.up;
        float startAngle = Vector2.Angle(Vector2.up, startUp);
        float time = startAngle / rotateBackSpeed;
        time = Mathf.Pow(time, 0.5f);
        float t = 0;

        while (t < time)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            float progress = t / time;
            spriteRenderer.transform.up = Vector2.LerpUnclamped(startUp, Vector2.up, rotateBackCurve.Evaluate(progress));
        }
    }

    IEnumerator FreeGround(float time)
    {
        float t = 0;
        shake.enabled = true;
        shake.BaseAmplitude = 0;

        while (t < time)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            shake.BaseAmplitude = freeGroundShakeCurve.Evaluate(t / time);
        }

        shake.enabled = false;
        shake.BaseAmplitude = 0;
        
        SetGrounded(false);
        SetOpened(true);

        justFreedGround = true;
        Invoke("SetJustFreedGroundFalse", justFreedGroundTime);
    }

    void SetJustFreedGroundFalse() => justFreedGround = false;
}


public interface IUmbrelloidTarget
{
    public void OnUmbrelloidHit(float dmg);
}
