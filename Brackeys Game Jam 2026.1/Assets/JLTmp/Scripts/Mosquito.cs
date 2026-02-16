using System.Collections.Generic;
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
    [SerializeField] bool avoidObstacles = true;
    [SerializeField, ShowField("avoidObstacles")] float avoidObstaclesRadius = 1;
    [SerializeField, ShowField("avoidObstacles")] int avoidObstaclesRaycastCount = 10;
    [SerializeField, ShowField("avoidObstacles")] LayerMask avoidObstaclesLayer;
    [SerializeField, ShowField("avoidObstacles")] float avoidObstaclesAcceleration = 1;

    Rigidbody2D rb;

    void OnDrawGizmosSelected()
    {   
        DrawAvoidObstacles();
    }

    void DrawAvoidObstacles()
    {
        Vector3 dir = Vector3.up;
        float stepAngle = 360f / avoidObstaclesRaycastCount;

        for (int i = 0; i < avoidObstaclesRaycastCount; i++)
        {
            RaycastHit2D h = Physics2D.Raycast(transform.position, dir, avoidObstaclesRadius, avoidObstaclesLayer);
            Vector2 lineEnd = h ? h.point : transform.position + dir * avoidObstaclesRadius;
            Gizmos.color = h ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, lineEnd);            
            dir =  Quaternion.Euler(0, 0, stepAngle) * dir;
        }

        if (PhysicExtension.TryRaycast2DCircle(transform.position, avoidObstaclesRadius, avoidObstaclesRaycastCount, avoidObstaclesLayer, out RaycastHit2D hit))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.05f);  
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
        if (avoidObstacles) AvoidObstacles();
        ApplyFriction();
    }

    void ApplyAcceleration()
    {
        Vector2 noiseValue = new(Mathf.PerlinNoise1D(noiseOffset.x + Time.time * noiseScale) * 2f - 0.9305f, 
                                 Mathf.PerlinNoise1D(noiseOffset.y + Time.time * noiseScale) * 2f - 0.9305f);

        rb.linearVelocity += acceleration * Time.fixedDeltaTime * noiseValue;
    }

    void AvoidObstacles()
    {
        if (!PhysicExtension.TryRaycast2DCircle(transform.position, avoidObstaclesRadius, avoidObstaclesRaycastCount, avoidObstaclesLayer, out RaycastHit2D hit))
            return;

        Vector2 toHitPoint = hit.point - (Vector2)transform.position;
        Vector2 direction = Vector2.Reflect(toHitPoint.normalized, hit.normal);
        rb.linearVelocity += avoidObstaclesAcceleration * Time.fixedDeltaTime * direction;
    }

    void ApplyFriction()
    {
        rb.linearVelocity *= 1 - friction * Time.fixedDeltaTime;
    }
}
