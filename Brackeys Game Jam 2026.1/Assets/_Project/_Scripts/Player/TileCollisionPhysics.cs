using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCollisionPhysics : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileDatabase tileDatabase;
    [SerializeField] private Health playerHealth;

    private void OnTriggerEnter2D(Collision2D other)
    {
        /*
        if (other.CompareTag("Ground"))
        {
            Vector3Int cellPosition = tilemap.WorldToCell(other.transform.position);
            Debug.Log($"Collided with tile at Position: {other.transform.position}, Cell Position: {cellPosition}");
            TileData tileData = tileDatabase.tileDatabase[tilemap.GetTile(cellPosition).name];
            Debug.Log($"Collided with tile: {tileData.tileName}");
            if (tileData != null && tileData.physicalMaterial != null)
            {
                other.GetComponent<Collider2D>().sharedMaterial = tileData.physicalMaterial;
                Debug.Log($"Applied physical material: {tileData.physicalMaterial.name}");
                if(tileData.name == "Lava Rule Tiles")
                {
                    playerHealth.Drain(4f); // Adjust the damage rate as needed
                }
            }
            if(tileData.physicalMaterial == null)
            {
                other.GetComponent<Collider2D>().sharedMaterial = null; // Reset to default material if no physical material is defined
            }
        }*/
    }
}
