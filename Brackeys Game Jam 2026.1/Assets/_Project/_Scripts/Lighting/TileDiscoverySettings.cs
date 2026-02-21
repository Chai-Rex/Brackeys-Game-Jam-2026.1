using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ScriptableObject that maps specific TileBase assets to custom fog discovery settings.
/// 
/// CREATE:
///   Right-click in Project window -> Create -> Fog of War -> Tile Discovery Settings
/// 
/// USAGE:
///   Add entries for each tile type you want custom behavior on.
///   Tiles not listed will use the FogOfWarManager's default exploredBrightness.
/// 
///   Example uses:
///   - Transparent/glass tiles: set exploredBrightness = 1.0 (always fully visible once seen)
///   - Underground ore: set exploredBrightness = 0.1 (very dark when explored)
///   - Background wall tiles: set exploredBrightness = 0.2 (dimmer than foreground)
/// </summary>
[CreateAssetMenu(fileName = "TileDiscoverySettings", menuName = "Fog of War/Tile Discovery Settings")]
public class TileDiscoverySettings : ScriptableObject {
    [System.Serializable]
    public class TileEntry {
        [Tooltip("The tile asset to configure. Drag from your Project window.")]
        public TileBase tile;

        [Tooltip("Brightness of this tile type when explored but out of current sight. " +
                 "0 = invisible, 1 = fully bright. Overrides FogOfWarManager.exploredBrightness.")]
        [Range(0f, 1f)]
        public float exploredBrightness = 0.35f;

        [Tooltip("If true, this tile type blocks line of sight regardless of the solidTilemap setting. " +
                 "Useful for non-solid tiles that should still block vision (e.g. dense foliage).")]
        public bool blocksLineOfSight = false;

        [Tooltip("If true, this tile type is always fully visible once discovered and never goes dark. " +
                 "Useful for special markers, quest tiles, etc.")]
        public bool alwaysVisible = false;
    }

    [Tooltip("Per-tile-type fog settings. Order does not matter.")]
    public List<TileEntry> entries = new List<TileEntry>();

    // Runtime lookup dictionary built on first access.
    private Dictionary<TileBase, TileEntry> _lookup;

    /// <summary>
    /// Returns the TileEntry for a given tile, or null if not configured.
    /// </summary>
    public TileEntry GetEntry(TileBase tile) {
        if (tile == null) return null;

        if (_lookup == null)
            BuildLookup();

        TileEntry entry;
        return _lookup.TryGetValue(tile, out entry) ? entry : null;
    }

    /// <summary>
    /// Returns the explored brightness for a tile, falling back to the given default.
    /// </summary>
    public float GetExploredBrightness(TileBase tile, float defaultBrightness) {
        TileEntry entry = GetEntry(tile);
        return entry != null ? entry.exploredBrightness : defaultBrightness;
    }

    /// <summary>
    /// Returns true if this tile type should block line of sight.
    /// </summary>
    public bool BlocksLineOfSight(TileBase tile) {
        TileEntry entry = GetEntry(tile);
        return entry != null && entry.blocksLineOfSight;
    }

    /// <summary>
    /// Returns true if this tile should always remain fully visible once discovered.
    /// </summary>
    public bool IsAlwaysVisible(TileBase tile) {
        TileEntry entry = GetEntry(tile);
        return entry != null && entry.alwaysVisible;
    }

    void BuildLookup() {
        _lookup = new Dictionary<TileBase, TileEntry>();
        foreach (TileEntry entry in entries) {
            if (entry.tile != null && !_lookup.ContainsKey(entry.tile))
                _lookup[entry.tile] = entry;
        }
    }

    // Rebuild lookup if entries change in the editor.
    void OnValidate() {
        _lookup = null;
    }
}