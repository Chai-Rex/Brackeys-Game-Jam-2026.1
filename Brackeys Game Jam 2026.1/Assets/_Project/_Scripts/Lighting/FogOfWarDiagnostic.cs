using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Attach to your FogOfWarManager GameObject to diagnose setup problems.
/// Press F1 in Play mode for a full report. Remove when everything works.
/// </summary>
public class FogOfWarDiagnostic : MonoBehaviour {
    public FogOfWarManager fogManager;
    public Material fogMaterial;

    [Tooltip("Press this key in Play mode to print a diagnostic report.")]
    public Key diagnosticKey = Key.T;

    void Start() {
        Invoke(nameof(PrintDiagnostic), 0.5f);
    }

    void Update() {
        if (Keyboard.current[diagnosticKey].isPressed)
            PrintDiagnostic();
    }

    void PrintDiagnostic() {
        Debug.Log("=== FOG OF WAR DIAGNOSTIC ===");

        if (fogManager == null) {
            Debug.LogError("[Diag] fogManager is NULL.");
            return;
        }

        // --- Player ---
        if (fogManager.player == null) {
            Debug.LogError("[Diag] FogOfWarManager.player is NULL. " +
                           "Drag your actual Player GameObject into the Player field.");
        } else {
            bool suspectSelf = fogManager.player.gameObject == fogManager.gameObject;
            Debug.Log("[Diag] Player: " + fogManager.player.name +
                      " pos=" + fogManager.player.position +
                      (suspectSelf ? " <-- WARNING: player field points to FogOfWarManager itself!" : ""));
        }

        // --- Tilemap ---
        if (fogManager.solidTilemap == null) {
            Debug.LogError("[Diag] solidTilemap is NULL.");
        } else {
            var bounds = fogManager.solidTilemap.cellBounds;
            int count = fogManager.solidTilemap.GetUsedTilesCount();
            Debug.Log("[Diag] solidTilemap: bounds=" + bounds + " tile count=" + count);

            if (count < 10)
                Debug.LogWarning("[Diag] solidTilemap has only " + count + " tiles. " +
                                 "solidTilemap controls map bounds AND LOS blocking. " +
                                 "If your terrain is on a different tilemap, assign that one instead.");
        }

        // --- Material and RenderTexture ---
        if (fogMaterial == null) {
            Debug.LogWarning("[Diag] fogMaterial not assigned to this diagnostic component.");
        } else {
            string shaderName = fogMaterial.shader != null ? fogMaterial.shader.name : "NULL";
            Debug.Log("[Diag] Material shader: " + shaderName);

            Texture fogTex = fogMaterial.GetTexture("_FogTex");
            if (fogTex == null) {
                Debug.LogError("[Diag] _FogTex is NULL on the material. " +
                               "The fog overlay will be invisible. " +
                               "Confirm the fogMaterial field on FogOfWarManager " +
                               "and on this Diagnostic both point to the same material asset.");
            } else {
                Debug.Log("[Diag] _FogTex: " + fogTex.name +
                          " " + fogTex.width + "x" + fogTex.height + " (ok)");
            }

            Vector4 rect = fogMaterial.GetVector("_FogWorldRect");
            Debug.Log("[Diag] _FogWorldRect: " + rect +
                      (rect == Vector4.zero ? " <-- WARNING: all zeros, map bounds not set" : " (ok)"));
        }

        // --- Discovery ---
        if (fogManager.player != null) {
            float disc = fogManager.GetDiscovery(fogManager.player.position);
            Vector2Int tile = fogManager.WorldToTile(fogManager.player.position);
            Debug.Log("[Diag] Player tile: " + tile + "   Discovery: " + disc +
                      (disc <= 0f ? " <-- 0: player may be outside map bounds" : " (ok)"));
        }

        // --- URP check ---
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        Debug.Log("[Diag] URP asset: " + (urpAsset != null ? urpAsset.name : "NOT FOUND - are you using URP?"));
        Debug.Log("[Diag] ACTION: Confirm Renderer has Full Screen Pass with Injection Point = After Rendering Post Processing.");

        Debug.Log("=== END DIAGNOSTIC ===");
    }
}