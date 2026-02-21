using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ScriptableObject mapping tile types to custom fog of war behavior.
///
/// CREATE: Right-click in Project -> Create -> Fog of War -> Tile Discovery Settings
/// </summary>
[CreateAssetMenu(fileName = "TileDiscoverySettings", menuName = "Fog of War/Tile Discovery Settings")]
public class TileDiscoverySettings : ScriptableObject {
    [System.Serializable]
    public class TileEntry {
        [Tooltip("The tile asset to configure.")]
        public TileBase tile;

        [Tooltip("Brightness when explored but out of current sight. " +
                 "0 = invisible, 1 = fully bright.")]
        [Range(0f, 1f)]
        public float exploredBrightness = 0.35f;

        [Tooltip("How much this tile blocks line of sight per tile crossed.\n" +
                 "0.0 = fully transparent (glass, air - does not block at all)\n" +
                 "0.5 = semi-transparent (thin foliage, fog)\n" +
                 "1.0 = fully opaque (solid rock - one tile kills all LOS strength)\n\n" +
                 "Only used if FogOfWarManager.useOpacityLOS is enabled.\n" +
                 "Tiles NOT in this list use FogOfWarManager.defaultTileOpacity.")]
        [Range(0f, 1f)]
        public float losOpacity = 1f;

        [Tooltip("If true, this tile is always fully visible once discovered.")]
        public bool alwaysVisible = false;
    }

    public List<TileEntry> entries = new List<TileEntry>();

    private Dictionary<TileBase, TileEntry> _lookup;

    public TileEntry GetEntry(TileBase tile) {
        if (tile == null) return null;
        if (_lookup == null) BuildLookup();
        TileEntry entry;
        return _lookup.TryGetValue(tile, out entry) ? entry : null;
    }

    public float GetExploredBrightness(TileBase tile, float defaultBrightness) {
        TileEntry e = GetEntry(tile);
        return e != null ? e.exploredBrightness : defaultBrightness;
    }

    /// <summary>
    /// Returns losOpacity [0,1] for this tile.
    /// 0 = transparent, 1 = fully opaque.
    /// Returns defaultOpacity if the tile has no entry.
    /// </summary>
    public float GetLosOpacity(TileBase tile, float defaultOpacity) {
        TileEntry e = GetEntry(tile);
        return e != null ? e.losOpacity : defaultOpacity;
    }

    public bool IsAlwaysVisible(TileBase tile) {
        TileEntry e = GetEntry(tile);
        return e != null && e.alwaysVisible;
    }

    void BuildLookup() {
        _lookup = new Dictionary<TileBase, TileEntry>();
        foreach (TileEntry e in entries)
            if (e.tile != null && !_lookup.ContainsKey(e.tile))
                _lookup[e.tile] = e;
    }

    void OnValidate() { _lookup = null; }
}