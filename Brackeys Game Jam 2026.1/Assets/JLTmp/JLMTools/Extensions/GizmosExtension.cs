using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GizmosExtension
{
    public static void DrawCircle(Vector3 center, float radius, int resolution = 30)
        => DrawCircle(center, Vector3.forward, radius, resolution);
        
    public static void DrawCircle(Vector3 center, Vector3 normal, float radius, int resolution = 30)
    {
        if (normal == Vector3.zero)
        {
            Debug.LogError("GizmosExtension.DrawCircle : normal == Vector3.zero");
            return;
        }

        if (resolution <= 1)
            return;

        Quaternion rot = Quaternion.LookRotation(normal);
        float drot = 360f / resolution;
        Vector3 p1, p2 = center + rot * Vector3.up * radius;

        for (int i = 0; i < resolution; i++)
        {
            p1 = p2;
            rot *= Quaternion.Euler(0, 0, drot);
            p2 = center + rot * Vector3.up * radius;
            Gizmos.DrawLine(p1, p2);
        }
    }

    public static void DrawArc(Vector3 center, Vector3 forward, Vector3 normal, float angle, float radius, int maxResolution = 60)
    {
        if (forward == Vector3.zero ||
            normal == Vector3.zero ||
            Mathf.Abs(Vector3.Dot(forward.normalized, normal.normalized)) > 0.999f)
        {
            Debug.LogError("GizmosExtension.DrawArc : Wrong directions.");
            return;
        }

        angle = Mathf.Abs(angle);
        angle = Mathf.Clamp(angle, 0, 360);

        if (angle == 0)
        {
            Gizmos.DrawLine(center, center + forward * radius);
            return;
        }
        
        int resolution = Mathf.CeilToInt(angle / 360 * maxResolution);
        resolution = Mathf.Max(resolution, 2);
        
        Vector3 right = Vector3.Cross(normal, forward);
        normal = Vector3.Cross(forward, right);

        Quaternion rot = Quaternion.LookRotation(forward, normal);
        rot *= Quaternion.Euler(0, -angle/2, 0);
        float drot = angle / resolution;

        Vector3 p1, p2 = center + rot * Vector3.forward * radius;
        Gizmos.DrawLine(center, p2);

        for (int i = 0; i < resolution; i++)
        {
            p1 = p2;
            rot *= Quaternion.Euler(0, drot, 0);
            p2 = center + rot * Vector3.forward * radius;
            Gizmos.DrawLine(p1, p2);
        }

        Gizmos.DrawLine(center, p2);
    }

    public static void DrawArrow(Vector2 start, Vector2 end, float headLength, float headAngle = 90)
        => DrawArrow(start, end, Vector3.forward, headLength, headAngle);

    public static void DrawArrow(Vector3 start, Vector3 end, Vector3 normal, float headLength, float headAngle = 90)
    {
        if (start == end || 
            normal == Vector3.zero)
            return;

        Vector3 endToStartDir = (start - end).normalized;
        normal.Normalize();

        if (Mathf.Abs(Vector3.Dot(normal, endToStartDir)) > 0.999f)
            return;

        Gizmos.DrawLine(start, end);
        Gizmos.DrawLine(end, end + Quaternion.LookRotation(endToStartDir, normal) * Quaternion.Euler(0, headAngle / 2, 0) * Vector3.forward * headLength);
        Gizmos.DrawLine(end, end + Quaternion.LookRotation(endToStartDir, normal) * Quaternion.Euler(0,-headAngle / 2, 0) * Vector3.forward * headLength);        
    }
}
