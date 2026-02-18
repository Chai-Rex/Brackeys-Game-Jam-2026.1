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
    private float offset = 0f;
    public bool isDrilling = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        drillCollider = drill.GetComponent<PolygonCollider2D>();
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Drill"].performed += ctx => StartDrilling();
        playerInput.actions["Drill"].canceled += ctx => isDrilling = false;
    }

    // Update is called once per frame
    void Update()
    {
        difference = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
        rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        drill.transform.rotation = Quaternion.Euler(0f, 0f, rotation_z + offset);
        if (isDrilling)
        {
            var hit = Physics2D.Raycast(transform.position, difference.normalized, 1000f, DestructibleLayer);
            Debug.DrawRay(transform.position, difference.normalized * 1000f, Color.red);
            if (hit.collider != null)
            {
                var tilemap = hit.collider.GetComponent<Tilemap>();
                var cellPosition = tilemap.WorldToCell(hit.point);
                tilemap.SetTile(cellPosition, null);
                Debug.Log("Hit: " + hit.collider.gameObject.name);
            }
        }
    }

    private void StartDrilling()
    {
        isDrilling = true;
        Debug.Log("Drilling Started");
    }

}
