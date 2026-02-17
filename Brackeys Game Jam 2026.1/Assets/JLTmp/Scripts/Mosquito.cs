using System.Linq;
using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Mosquito : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] float acceleration = 10;
    [SerializeField] float friction = 10;
    [SerializeField] float noiseScale = 1;
    Vector2 noiseOffset;

    [Space(15)]
    [SerializeField] bool avoidObstacles = true; // TODO opti : static AvoidObstaclesMap
    [SerializeField, ShowField("avoidObstacles")] float avoidObstaclesRadius = 1;
    [SerializeField, ShowField("avoidObstacles")] int avoidObstaclesRaycastCount = 10;
    [SerializeField, ShowField("avoidObstacles")] float avoidObstaclesAcceleration = 1;
    [SerializeField, ShowField("avoidObstacles")] LayerMask obstaclesLayer;


    [Space(15)]
    [SerializeField] bool searchTarget = true;
    [SerializeField, ShowField("searchTarget")] float searchTargetRadius = 1;
    [SerializeField, ShowField("searchTarget")] float targetAcceleration = 1;
    [SerializeField, ShowField("searchTarget")] float targetLandCoef = 10;
    [SerializeField, ShowField("searchTarget")] Vector2 targetRepulsionSpeedRange = new(2, 3);
    [SerializeField, ShowField("searchTarget")] float unlockTargetTime = 2;
    [SerializeField, ShowField("searchTarget"), ReadOnly] MosquitoTarget target = null;
    bool targetLocked = false;

    Rigidbody2D rb;

    void OnDrawGizmosSelected()
    {   
        if (avoidObstacles)
            DrawAvoidObstacles();

        if (searchTarget)
            DrawSearchTargetRadius();
    }

    void DrawAvoidObstacles()
    {
        Vector3 dir = Vector3.up;
        float stepAngle = 360f / avoidObstaclesRaycastCount;

        for (int i = 0; i < avoidObstaclesRaycastCount; i++)
        {
            RaycastHit2D h = Physics2D.Raycast(transform.position, dir, avoidObstaclesRadius, obstaclesLayer);
            Vector2 lineEnd = h ? h.point : transform.position + dir * avoidObstaclesRadius;
            Gizmos.color = h ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, lineEnd);            
            dir =  Quaternion.Euler(0, 0, stepAngle) * dir;
        }

        if (PhysicExtension.TryRaycast2DCircle(transform.position, avoidObstaclesRadius, avoidObstaclesRaycastCount, obstaclesLayer, out RaycastHit2D hit))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.05f);  
        }
    }

    void DrawSearchTargetRadius()
    {
        Gizmos.color = Color.yellow;
        GizmosExtension.DrawCircle(transform.position, Vector3.forward, searchTargetRadius);
    }

    void OnValidate()
    {
        if (!searchTarget)
        {
            target = null;
            targetLocked = false;
            CancelInvoke("UnlockTarget");
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        noiseOffset = RandomExtension.RandomVector2() * 1000;
    }

    void FixedUpdate()
    {
        ApplyAcceleration();

        if (avoidObstacles) 
            AvoidObstacles();

        if (searchTarget)
        {
            UpdateTarget();
            ApplyTargetAcceleration();
            ApplyTargetLand();
        }
        
        ApplyFriction();
    }

    void ApplyAcceleration()
    {
        Vector2 noiseValue = new(Mathf.PerlinNoise1D(noiseOffset.x + Time.time * noiseScale) * 2f - 0.9305f, 
                                 Mathf.PerlinNoise1D(noiseOffset.y + Time.time * noiseScale) * 2f - 0.9305f);

        rb.linearVelocity += acceleration * Time.fixedDeltaTime * noiseValue;
    }

    void ApplyFriction()
    {
        rb.linearVelocity *= 1 - friction * Time.fixedDeltaTime;
    }

    void AvoidObstacles()
    {
        if (target ||
            !PhysicExtension.TryRaycast2DCircle(transform.position, avoidObstaclesRadius, avoidObstaclesRaycastCount, obstaclesLayer, out RaycastHit2D hit))
            return;

        Vector2 toHitPoint = hit.point - (Vector2)transform.position;
        Vector2 direction = Vector2.Reflect(toHitPoint.normalized, hit.normal);
        rb.linearVelocity += avoidObstaclesAcceleration * Time.fixedDeltaTime * direction;
    }

    void SetTarget(MosquitoTarget target)
    {
        if (this.target == target)
            return;

        this.target?.OnStartMoving.Remove(OnTargetStartMoving);

        if (this.target)
            TargetRepulsion();

        this.target = target;
        this.target?.OnStartMoving.Add(OnTargetStartMoving);
    }

    void UpdateTarget()
    {
        if (!searchTarget ||
            targetLocked ||
            MosquitoTarget.TargetsCount == 0)
        {
            SetTarget(null);
            return;
        }

        MosquitoTarget closestTarget = MosquitoTarget.Targets.Where((t) => t.Rigidbody2D).OrderBy((t) => (transform.position - t.transform.position).sqrMagnitude).First();
        float sqrDist = (transform.position - closestTarget.transform.position).sqrMagnitude;
        float sqrRadius = searchTargetRadius * searchTargetRadius;

        if (sqrDist <= sqrRadius)
        {
            SetTarget(closestTarget);
            return;
        }

        closestTarget = MosquitoTarget.Targets.Where((t) => !t.Rigidbody2D).OrderBy((t) => (transform.position - t.transform.position).sqrMagnitude).First();
        sqrDist = (transform.position - closestTarget.transform.position).sqrMagnitude;

        SetTarget(sqrDist <= sqrRadius ?
            closestTarget : 
            null);
    }

    void ApplyTargetAcceleration()
    {
        if (!target)
            return;

        Vector2 toTarget = target.transform.position - transform.position;

        if (toTarget.sqrMagnitude < target.SqrInRadius)
            return;

        rb.linearVelocity += targetAcceleration * Time.fixedDeltaTime * toTarget.normalized;
    }

    void ApplyTargetLand()
    {
        if (!target)
            return;

        Vector2 toTarget = target.transform.position - transform.position;
        float sqrDist = toTarget.sqrMagnitude;

        if (sqrDist > target.SqrOutRadius)
            return;

        rb.linearVelocity *= 1 - targetLandCoef * Time.fixedDeltaTime;
    }

    void TargetRepulsion()
    {
        if (!target)
            return;

        Vector2 toTarget = target.transform.position - transform.position;
        
        if (toTarget == Vector2.zero)
            toTarget.y -= 0.01f;

        float sqrDist = toTarget.sqrMagnitude;

        if (sqrDist > target.SqrOutRadius)
            return;

        float dist = Mathf.Sqrt(sqrDist);
        Vector2 dir = toTarget / dist;
        rb.linearVelocity = -dir * targetRepulsionSpeedRange.RandomInRange();
    }

    void OnTargetStartMoving()
    {
        TargetRepulsion();
        SetTarget(null);
        targetLocked = true;
        Invoke("UnlockTarget", unlockTargetTime);
    }

    void UnlockTarget() => targetLocked = false;
}