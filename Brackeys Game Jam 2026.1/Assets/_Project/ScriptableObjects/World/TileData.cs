using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TileData : ScriptableObject
{
    public string tileName;
    public bool isDestructible;
    public int durability;
    public int pointValue;
    public PhysicsMaterial2D physicalMaterial;
}
