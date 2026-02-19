using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PhysicExtension
{
    public static bool TryRaycast2D(Vector2 start, Vector2 direction, float length, LayerMask layer, out RaycastHit2D hit)
    {
        hit = Physics2D.Raycast(start, direction, length, layer);
        return hit;
    }

    public static bool TryRaycast2DCircle(Vector2 center, float radius, int nbRaycast, LayerMask layer, out RaycastHit2D hit)
    {
        hit = Raycast2DCircle(center, radius, nbRaycast, layer);
        return hit;
    }

    public static RaycastHit2D Raycast2DCircle(Vector2 center, float radius, int nbRaycast, LayerMask layer)
    {
        Vector2 dir = Vector3.up;
        float stepAngle = 360f / nbRaycast;
        List<RaycastHit2D> hits = new();

        for (int i = 0; i < nbRaycast; i++)
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
