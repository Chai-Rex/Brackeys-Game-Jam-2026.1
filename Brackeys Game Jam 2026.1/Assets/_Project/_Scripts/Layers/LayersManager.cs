using UnityEngine;

[CreateAssetMenu(fileName = "LayersManager", menuName = "ScriptableObjects/Managers/LayersManager")]
public class LayersManager : ScriptableObject {

    public LayerMask Player;
    public LayerMask Enemies;
    public LayerMask Environment;
    public LayerMask Interactables;

}
