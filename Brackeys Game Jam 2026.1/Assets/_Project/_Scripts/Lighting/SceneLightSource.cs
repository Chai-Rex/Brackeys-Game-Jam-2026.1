using UnityEngine;

/// <summary>
/// Place on any GameObject to make it a stationary scene light source
/// for the fog of war system.
///
/// ACTIVATION:
///   A scene light activates when the outer radius of the player's light
///   (or the outer radius of another already-active scene light) overlaps
///   with the outer radius of this light. Once activated it stays permanently
///   on - the area it illuminated remains discovered forever.
///
/// CHAIN REACTION:
///   Once this light activates it becomes a new source that can trigger
///   further scene lights whose radii overlap with its own outer radius.
///   This creates a flood-fill effect through connected lit cave networks.
///
/// ALWAYS ACTIVE:
///   Tick alwaysActive for lights that should be on from the start
///   regardless of player proximity (e.g. lights near the spawn).
///
/// SETUP:
///   1. Place a GameObject at your light's world position.
///   2. Attach this component.
///   3. Set radius and diffusionWidth to match your visual light.
///   4. FogOfWarManager collects all SceneLightSource components on Start.
///      For runtime-spawned lights call fogManager.RegisterLightSource(this).
/// </summary>
public class SceneLightSource : MonoBehaviour {
    [Tooltip("Inner radius in tiles where this light gives 100% visibility.")]
    public float radius = 5f;

    [Tooltip("Extra tiles beyond radius where this light fades to 0. " +
             "The outer edge (radius + diffusionWidth) is what triggers other nearby lights.")]
    public float diffusionWidth = 2f;

    [Tooltip("If true this light is active from the very start, revealing its area immediately. " +
             "Use for lights near the player spawn.")]
    public bool alwaysActive = false;

    /// <summary>
    /// Set to true by FogOfWarManager when this light is activated.
    /// Once true it never goes back to false - activation is permanent.
    /// </summary>
    [System.NonSerialized]
    public bool isActivated = false;

    /// <summary>
    /// The full outer radius that determines when this light triggers others
    /// and when others trigger it.
    /// </summary>
    public float OuterRadius => radius + diffusionWidth;

    [Tooltip("Must match FogOfWarManager.tileSize so gizmos draw at the correct world scale.")]
    public float tileSize = 0.5f;

    void OnDrawGizmosSelected() {
        // Inner radius - full brightness zone.
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
        DrawCircle(transform.position, radius * tileSize);

        // Outer radius - diffusion and trigger zone.
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.2f);
        DrawCircle(transform.position, (radius + diffusionWidth) * tileSize);
    }

    void DrawCircle(Vector3 center, float r) {
        float step = (360f / 40) * Mathf.Deg2Rad;
        Vector3 prev = center + new Vector3(r, 0f, 0f);
        for (int i = 1; i <= 40; i++) {
            float a = i * step;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}