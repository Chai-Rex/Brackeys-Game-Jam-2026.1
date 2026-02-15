using UnityEngine;

public interface IInteractable {
    /// <summary>
    /// The verb to display on the HUD (e.g., "Grab", "Pickup", "Talk")
    /// </summary>
    string InteractionVerb { get; }

    /// <summary>
    /// Called when the player interacts with this object
    /// </summary>
    void OnInteract(GameObject i_interactor);

    /// <summary>
    /// Optional: Called when the player starts looking at this interactable
    /// </summary>
    void OnLookEnter(GameObject i_looker) { }

    /// <summary>
    /// Optional: Called when the player stops looking at this interactable
    /// </summary>
    void OnLookExit(GameObject i_looker) { }

    /// <summary>
    /// Optional: Returns the transform of the interactable (useful for highlighting)
    /// </summary>
    Transform GetTransform() { return null; }
}
