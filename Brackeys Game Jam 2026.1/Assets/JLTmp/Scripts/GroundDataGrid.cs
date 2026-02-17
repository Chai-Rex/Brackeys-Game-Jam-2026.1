using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GroundDataGrid : Singleton<GroundDataGrid>
{
    Dictionary<Vector2Int, GroundData> grid = new();
    [SerializeField] float cellSize = 1;
    [SerializeField] int nbRaycast = 13;
    [SerializeField] int raycastLength = 100;
    [SerializeField] LayerMask groundLayer;

    [Header("Gizmos")]
    [SerializeField] Gradient gizmosGradient;
    [SerializeField] float gizmosMaxGradientDistance = 1;

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
        RaycastHit2D hit = PhysicExtension.Raycast2DCircle(position, raycastLength, nbRaycast, groundLayer);
        data = new(position, coor, hit);
        grid.Add(coor, data);
        return data;
    }

    void OnDrawGizmosSelected()
    {
        DrawCellSize();
        DrawGrid();
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

    void DrawGrid()
    {
        // Vector3 cubeSize = new Vector3(1, 1, 0) * cellSize;
        // Color insideColor = gizmosGradient.Evaluate(0);
        // insideColor.a = 0.2f;

        // foreach (KeyValuePair<Vector2Int, GroundData> kv in grid.Clone())
        // {
        //     Vector3 position = CoorToPosition(kv.Key);

        //     if (!kv.Value.HasHit || 
        //         kv.Value.OutDirection == Vector2.zero)
        //     {
        //         Gizmos.color = !kv.Value.HasHit ? Color.gray : insideColor;
        //         Gizmos.DrawCube(position, cubeSize);
        //     }
        //     else {
        //         Gizmos.color = gizmosGradient.Evaluate(kv.Value.GroundDistance / gizmosMaxGradientDistance);
        //         Gizmos.DrawLine(position - (Vector3)kv.Value.OutDirection/2 * cellSize, position + (Vector3)kv.Value.OutDirection/2 * cellSize);
        //     }
        // }
    }
}

public struct GroundData
{
    Vector2 position;
    Vector2Int coor;
    RaycastHit2D hit;
    bool hasHit;
    bool isInside;
    Vector2 toHitPoint;
    float groundDistance;
    Vector2 groundDirection;
    Vector2 outDirection;
    bool outDirectionInitialized;

    public Vector2 Position => position;
    public Vector2Int Coor => coor;
    public bool HasHit => hasHit;
    public bool IsInside => isInside;
    public RaycastHit2D Hit => hit;
    public Vector2 ToHitPoint => toHitPoint;
    public float GroundDistance => groundDistance;
    public Vector2 GroundDirection => groundDirection;
    public Vector2 OutDirection
    {
        get {
            if (!outDirectionInitialized)
                InitializeOutDirection();

            return outDirection;
        }
    }

    public GroundData(Vector2 position, Vector2Int coor, RaycastHit2D hit)
    {
        this.position = position;
        this.coor = coor;
        this.hit = hit;
        hasHit = hit;
        outDirection = Vector2.zero;
        outDirectionInitialized = false;

        if (hasHit) {
            toHitPoint = hit.point - position;
            groundDistance = toHitPoint.magnitude;
            isInside = groundDistance == 0;
            groundDirection = !isInside ? toHitPoint / groundDistance : Vector2.zero;
        } else {
            toHitPoint = Vector2.zero;
            groundDistance = 0;
            isInside = false;
            groundDirection = Vector2.zero;
        }
    }

    void InitializeOutDirection()
    {
        if (!hasHit)
            outDirection = Vector2.zero;

        else if (!isInside)
            outDirection = -groundDirection;
        
        else {
            Vector2 sum = Vector2.zero;

            for (int y = coor.y-1; y <= coor.y+1; y++) {
                for (int x = coor.x-1; x <= coor.x+1; x++)
                {
                    GroundData data = GroundDataGrid.Instance.GetData(x, y);
                    if (data.hasHit && 
                        !data.isInside)
                        sum += data.OutDirection;
                }
            }

            if (sum != Vector2.zero)
                outDirection = sum.normalized;
        }

        outDirectionInitialized = true;
    }
}