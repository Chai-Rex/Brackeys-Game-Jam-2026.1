using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GroundDataGrid : Singleton<GroundDataGrid>
{
    Dictionary<Vector2Int, GroundData> grid = new();
    [SerializeField] float cellSize = 1;
    [SerializeField] int nbRaycast = 8;
    [SerializeField] int raycastLength = 100;
    [SerializeField] LayerMask groundLayer;

    [Header("Gizmos")]
    [SerializeField] Gradient gizmosGradient;
    [SerializeField] float gizmosMaxGradientDistance = 2;
    [SerializeField] DrawData gizmosDrawData = DrawData.OutDirection;

    enum DrawData
    {
        HasHit, 
        HitPoint,
        HitNormal, 
        GroundDirection,
        IsInside,
        OutDirection
    }
    Vector2 CoorToPosition(Vector2Int coor) => (Vector2)coor * cellSize;
    Vector2 CoorToPosition(int x, int y) => new Vector2(x, y) * cellSize;
    Vector2Int PositionToCoor(Vector2 position) => new Vector2Int(
        Mathf.RoundToInt(position.x / cellSize), 
        Mathf.RoundToInt(position.y / cellSize));

    public GroundData GetData(Vector2 position)
        => GetData(PositionToCoor(position));

    public GroundData GetData(int x, int y) 
        => GetData(new Vector2Int(x, y));

    public GroundData GetData(Vector2Int coor)
    {
        if (grid.TryGetValue(coor, out GroundData data))
            return data;

        Vector2 position = CoorToPosition(coor);
        data = new(position, coor, cellSize, raycastLength, nbRaycast, groundLayer);
        grid.Add(coor, data);
        return data;
    }

    void OnDrawGizmosSelected()
    {
        // if (Application.isPlaying)
        //     GetData(RandomExtension.RandomVector2(-5, 5));

        DrawCellSize();
        DrawGrid(gizmosDrawData);
    }

    void DrawCellSize()
    {
        Gizmos.color = Color.grey;
        Vector3 cubeSize = new Vector3(1, 1, 0) * cellSize;
        int minMax = 5;

        for (int y = -minMax; y <= minMax; y++)
            for (int x = -minMax; x <= minMax; x++)
                Gizmos.DrawWireCube(CoorToPosition(x, y), cubeSize);
    }

    void DrawGrid(DrawData drawData)
    {
        Vector3 cubeSize = new Vector3(1, 1, 0) * cellSize;
        Color insideColor = gizmosGradient.Evaluate(0);
        insideColor.a = 0.2f;

        foreach (KeyValuePair<Vector2Int, GroundData> kv in grid.Clone())
        {
            Vector3 position = CoorToPosition(kv.Key);
            GroundData data = kv.Value;

            if (!data.HasHit)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(position, cubeSize);
                continue;
            }

            switch (drawData)
            {
                case DrawData.HasHit :
                    Gizmos.color = new Color(0, 1, 0, 0.3f);
                    Gizmos.DrawCube(position, cubeSize);
                    break;  

                case DrawData.HitPoint : 
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(data.HitPoint, 0.05f);
                    break;   

                case DrawData.HitNormal :
                    if (data.IsInside)
                        break;
                    Gizmos.color = Color.white;
                    GizmosExtension.DrawArrow(data.HitPoint, data.HitPoint + data.HitNormal * cellSize, cellSize/4);
                    break;
                
                case DrawData.GroundDirection : 
                    if (data.IsInside)
                        break;
                    Gizmos.color = gizmosGradient.Evaluate(data.GroundDistance / gizmosMaxGradientDistance);
                    GizmosExtension.DrawArrow(
                        position - (Vector3)data.GroundDirection / 2 * cellSize, 
                        position + (Vector3)data.GroundDirection / 2 * cellSize, 
                        cellSize/4);
                    break;   
                
                case DrawData.IsInside : 
                    Gizmos.color = data.IsInside ? new Color(1, 0, 0, 0.3f) : new Color(0, 1, 0, 0.3f);
                    Gizmos.DrawCube(position, cubeSize);
                    break;   
                
                case DrawData.OutDirection :
                    Gizmos.color = gizmosGradient.Evaluate(data.GroundDistance / gizmosMaxGradientDistance);
                    GizmosExtension.DrawArrow(
                        position - (Vector3)data.OutDirection / 2 * cellSize, 
                        position + (Vector3)data.OutDirection / 2 * cellSize, 
                        cellSize/4);
                    break;   
            }
        }
    }
}

public struct GroundData
{
    Vector2 position;
    Vector2Int coor;

    bool hasHit;
    Vector3 hitPoint;
    Vector3 hitNormal;

    float groundDistance;
    Vector2 groundDirection;

    bool isInside;
    Vector2 outDirection;



    public Vector2 Position => position;
    public Vector2Int Coor => coor;

    public bool HasHit => hasHit;
    public Vector3 HitPoint => hitPoint;
    public Vector3 HitNormal => hitNormal;

    public float GroundDistance => groundDistance;
    public Vector2 GroundDirection => groundDirection;

    public bool IsInside => isInside;
    public Vector2 OutDirection => outDirection;



    public GroundData(Vector2 position, Vector2Int coor, float cellSize, float raycastLength, int nbRaycast, LayerMask groundLayer)
    {
        this.position = position;
        this.coor = coor;
        RaycastHit2D hit = PhysicExtension.Raycast2DCircle(position, raycastLength, nbRaycast, groundLayer);
        hasHit = hit;

        if (hasHit) 
        {
            hitPoint = hit.point;
            hitNormal = hit.normal;
            Vector3 toHitPoint = hit.point - position;
            groundDistance = toHitPoint.magnitude;
            isInside = groundDistance == 0;

            if (!isInside)
            {
                groundDirection = toHitPoint / groundDistance;
                outDirection = -groundDirection;
            } 
            
            else {
                groundDirection = Vector2.zero;
                outDirection = TryReverseSquareRaycast(position, cellSize, nbRaycast, groundLayer, out RaycastHit2D hit2) && hit2.point != position ? 
                    (hit2.point - position).normalized :
                    Vector2.zero;
            }
        } 
        
        else {
            hitPoint = hit.point;
            hitNormal = hit.normal;
            groundDistance = 0;
            groundDirection = Vector2.zero;
            isInside = false;
            outDirection = Vector2.zero;
        }
    }

    static bool TryReverseSquareRaycast(Vector2 center, float size, int nbRaycast, LayerMask layer, out RaycastHit2D hit)
    {
        Vector2 dir = Vector2.up;
        float stepAngle = 360f / nbRaycast;
        List<RaycastHit2D> hits = new();

        for (int i = 0; i < nbRaycast; i++)
        {
            dir = Quaternion.Euler(0, 0, stepAngle) * dir;
            float max = Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            Vector2 start = center + dir * size / max;
            Vector2 end = center;
            Vector2 startToEnd = end - start;

            if (PhysicExtension.TryRaycast2D(start, startToEnd, startToEnd.magnitude, layer, out RaycastHit2D h) &&
                h.point != start)
                hits.Add(h);
        }

        hit = hits.Count > 0 ? 
            hits.OrderBy(h => (h.point - center).sqrMagnitude).First() : 
            default;

        return hit;
    }
}