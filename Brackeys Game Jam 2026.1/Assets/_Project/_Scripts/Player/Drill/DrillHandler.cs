using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class DrillHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    public LayerMask DestructibleLayer;

    public GameObject drill;
    private PolygonCollider2D drillCollider;
    private Vector3 difference;
    private float rotation_z;
    private Vector3Int previousCellSelected;
    [SerializeField]private float Drilloffset;
    [SerializeField]private float drillRange;
    [SerializeField]private float drillDelay;
    public bool isDrilling = false;
    private bool coroutineRunning = false;
    //Only for Point To Click Drilling
    [SerializeField]private Tilemap tilemap;
    private TileDatabase tileDatabase;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //For Drill Based on Collider
        //drillCollider = drill.GetComponent<PolygonCollider2D>();
        playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput == null) {
            Debug.LogError("PlayerInput Not found");

        }
        tileDatabase = tilemap.GetComponent<TileDatabase>();
        playerInput.actions["Drill"].performed += ctx => isDrilling = true;
        playerInput.actions["Drill"].canceled += ctx => { isDrilling = false; previousCellSelected = Vector3Int.zero; };
    }

    // Update is called once per frame
    void Update()
    {
        //Only for collider based Drilling
        //Collider Drill Code Attached to Tilemap Collision in DestructibleTileBehavior Script
        //rotateDrill();
        if (isDrilling)
        {
            //The following is for Point To Click Drilling
            PointToClickDrill();
            //The following is raycast Based Drilling
            //DrillRaycast();
        }
    }

    private void rotateDrill()
    {
        if(drill.activeSelf == false)
        {
            drill.SetActive(true);
        }
        //Drill Object Rotation for collider based Drilling
        difference = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
        rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        drill.transform.rotation = Quaternion.Euler(0f, 0f, rotation_z + Drilloffset);
    }

    private void PointToClickDrill()
    {
        Vector3 mouseWorldPosition = GetWorldPositionOnPlane();
        if(Vector3.Distance(transform.position, mouseWorldPosition) > drillRange)
        {
            return;
        }
        if (coroutineRunning)
        {
            return;
        }
        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPosition);
        tileData tileData = GetTileData(tilemap, cellPosition);
        if (tileData == null || tileData.isDestructible == false)
        {
            return;
        }
        StartCoroutine(DrillDelay(tilemap, tileData, cellPosition));
    }

    private void DrillRaycast()
    {
        var hit = Physics2D.Raycast(transform.position, Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), drillRange, DestructibleLayer);
        Debug.DrawRay(transform.position, Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()).normalized * drillRange, Color.red);
        if (hit.collider != null)
        {
            var tilemap = hit.collider.GetComponent<Tilemap>();
            var cellPosition = tilemap.WorldToCell(hit.point);
            //Instantiate(drill, cellPosition, Quaternion.identity);
            Debug.Log("Tile at Cell Position Before: " + tilemap.GetTile(cellPosition)?.name ?? "None");
            DestroyTile(tilemap, cellPosition);
            Debug.Log("Hit: " + hit.collider.gameObject.name);
            Debug.Log("Hit Point: " + hit.point);
            Debug.Log("Cell Position: " + cellPosition);
            Debug.Log("Tile at Cell Position After: " + tilemap.GetTile(cellPosition)?.name ?? "None");
        }
        else
        {
            Debug.Log("No hit detected.");
        }
    }

    private TileData GetTileData(Tilemap tilemap, Vector3Int cellPosition)
    {
        TileBase tile = tilemap.GetTile(cellPosition);
        if (tile != null)
        {
            Debug.Log("Tile at " + cellPosition + ": " + tile.name);
            Debug.Log("Tile Instance ID: " + tile.GetInstanceID());
                if (tileDatabase.tileDatabase.TryGetValue(tile.name, out TileData tileData))
                {
                    Debug.Log("Tile Data for " + tile.name + ":");
                    Debug.Log("Is Destructible: " + tileData.isDestructible);
                    Debug.Log("Durability: " + tileData.durability);
                    Debug.Log("Physical Material: " + tileData.physicalMaterial);
                    return tileData;
                }
                else
                {
                    Debug.Log("No Tile Data found for " + tile.name);
                }
        }
        else
        {
            Debug.Log("No tile at " + cellPosition);
        }
    }

    private void DestroyTile(Tilemap tilemap, TileData tileData, Vector3Int cellPosition)
    {
        tilemap.SetTile(cellPosition, null);
        //Debug.Log($"{cellPosition} destroyed");
    }

    private System.Collections.IEnumerator DrillDelay(Tilemap tilemap, TileData tileData, Vector3Int cellPosition)
    {
        //Debug.Log("Wait function started");
        coroutineRunning = true;
        float startTime = Time.time;
        Vector3Int startingCellPosition = cellPosition;
        while (Time.time - startTime < (drillDelay * tileData.durability))
        {
            if (isDrilling == false || cellPosition != startingCellPosition)
            {
                //Debug.Log("Interupted Wait");
                startTime = Time.time;
                coroutineRunning = false;
                yield break;
            }
            yield return null; //or WaitForEndOfFrame() etc
        }
        DestroyTile(tilemap, tileData, cellPosition);
        coroutineRunning = false;
        //Debug.Log("Wait function completed");
    }

    public Vector3 GetWorldPositionOnPlane(float z = 0f) {
        Camera cam = Camera.main;

        // Mouse position MUST stay in screen space
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Ray ray = cam.ScreenPointToRay(mouseScreenPos);

        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, z));

        if (plane.Raycast(ray, out float distance)) {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

}
