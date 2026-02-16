using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PhysicExtension
{
    public static bool TryRaycast2DCircle(Vector2 center, float radius, int raycastCount, LayerMask layer, out RaycastHit2D hit)
    {
        hit = Raycast2DCircle(center, radius, raycastCount, layer);
        return hit;
    }

    public static RaycastHit2D Raycast2DCircle(Vector2 center, float radius, int raycastCount, LayerMask layer)
    {
        Vector2 dir = Vector3.up;
        float stepAngle = 360f / raycastCount;
        List<RaycastHit2D> hits = new();

        for (int i = 0; i < raycastCount; i++)
        {
            RaycastHit2D h = Physics2D.Raycast(center, dir, radius, layer);
            if (h) hits.Add(h);
            dir =  Quaternion.Euler(0, 0, stepAngle) * dir;
        }

        return hits.Count > 0 ? 
            hits.OrderBy(h => (h.point - center).sqrMagnitude).First() : 
            default;
    }
}
