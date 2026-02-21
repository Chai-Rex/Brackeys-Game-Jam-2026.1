using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Fog of War Manager - quad overlay approach.
///
/// Features:
///   - Discovery-based fog using a RenderTexture sampled by a world-space quad.
///   - Chain-reaction scene lights: SceneLightSource components in the scene
///     activate when the player's light (or another active scene light) reaches
///     their tile, then contribute their own reveal pass, potentially activating
///     further lights in a flood-fill chain.
///   - OnTileBroken / OnTilePlaced for runtime tilemap changes.
///   - TileDiscoverySettings for per-tile-type brightness.
///   - Save/load via SerializeDiscoveryMap / DeserializeDiscoveryMap.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FogOfWarManager : MonoBehaviour {
    // ===========================================================
    //  Inspector
    // ===========================================================

    [Header("Required References")]
    [Tooltip("Your terrain tilemap. Used for map bounds detection and LOS blocking.")]
    public Tilemap solidTilemap;

    [Tooltip("The player Transform (your character, not the camera).")]
    public Transform player;

    [Tooltip("Material using FogOfWar.shader.")]
    public Material fogMaterial;

    [Header("Optional")]
    [Tooltip("Per-tile-type brightness settings. Leave empty to use default for all tiles.")]
    public TileDiscoverySettings tileSettings;

    [Tooltip("Additional tilemaps for per-tile brightness lookups (background, decoration, etc).")]
    public Tilemap[] additionalTilemaps;

    [Header("Tile Size")]
    [Tooltip("Size of one tile in world units. 0.5 for a 0.5x0.5 grid.")]
    public float tileSize = 0.5f;

    [Header("Fog Settings")]
    [Range(0f, 1f)]
    [Tooltip("Brightness of tiles explored but currently out of sight. 0=black, 1=full.")]
    public float exploredBrightness = 0.35f;

    [Tooltip("Radius in tiles where the player light gives 100% visibility.")]
    public float visibleRadius = 8f;

    [Tooltip("Extra tiles beyond visibleRadius where player light fades to 0.")]
    public float diffusionWidth = 3f;

    [Header("Surface")]
    [Tooltip("If true, all tiles above the topmost solid tile in each column start fully explored.")]
    public bool revealSurfaceOnStart = true;

    [Tooltip("How many solid tiles below the surface top to also reveal. " +
             "0 = only air above ground. 1 = surface tile + air. Default 0.")]
    public int surfaceRevealPaddingTiles = 0;

    [Header("Performance")]
    [Range(1, 4)]
    public int textureDownscale = 1;

    // ===========================================================
    //  Private State
    // ===========================================================

    private float[] _discoveryMap;
    private float[] _tileExploredBrightness;
    private bool[] _tileAlwaysVisible;

    // Best visibility written this reveal pass across player + all scene lights.
    // -1 means not lit this frame.
    private float[] _frameVisibility;

    private Vector2Int _mapSize;
    private Vector2 _mapOrigin;
    private Vector2Int _cellOffset;

    private RenderTexture _fogRT;
    private Texture2D _fogUploadTex;
    private Color[] _fogPixels;

    private Vector2Int _lastPlayerTile = new Vector2Int(int.MinValue, int.MinValue);
    private List<Vector2Int> _lineBuffer = new List<Vector2Int>(256);
    private HashSet<Vector2Int> _solidCache = new HashSet<Vector2Int>();
    private List<SceneLightSource> _sceneLights = new List<SceneLightSource>();

    private Camera _cam;
    private MeshRenderer _meshRenderer;

    private static readonly int ShaderFogTex = Shader.PropertyToID("_FogTex");
    private static readonly int ShaderFogWorldRect = Shader.PropertyToID("_FogWorldRect");
    private static readonly int ShaderQuadWorldRect = Shader.PropertyToID("_QuadWorldRect");

    // ===========================================================
    //  Unity Lifecycle
    // ===========================================================

    void Start() {
        _cam = Camera.main;
        DetectMapBounds();
        RebuildSolidCache();
        InitArrays();
        InitFogTexture();
        BuildOverlayQuad();
        CollectSceneLights();
        RevealSurface();
    }

    void OnDestroy() {
        if (_fogRT != null) { _fogRT.Release(); _fogRT = null; }
    }

    void LateUpdate() {
        if (player == null || _cam == null) return;

        UpdateQuadTransform();

        Vector2Int playerTile = WorldToTile(player.position);
        if (playerTile == _lastPlayerTile) return;
        _lastPlayerTile = playerTile;

        RunFullRevealPass(playerTile);
    }

    // ===========================================================
    //  Scene Light Registration
    // ===========================================================

    void CollectSceneLights() {
        _sceneLights.Clear();
        SceneLightSource[] found = FindObjectsByType<SceneLightSource>(FindObjectsSortMode.None);
        foreach (SceneLightSource s in found) {
            _sceneLights.Add(s);
            // alwaysActive lights are permanently on from the start.
            if (s.alwaysActive) {
                s.isActivated = true;
                RevealFromPoint(WorldToTile(s.transform.position), s.radius, s.diffusionWidth);
            }
        }
        Debug.Log("[FogOfWar] Found " + _sceneLights.Count + " scene light sources.");
    }

    /// <summary>Call this if you spawn a SceneLightSource at runtime.</summary>
    public void RegisterLightSource(SceneLightSource source) {
        if (!_sceneLights.Contains(source))
            _sceneLights.Add(source);
    }

    /// <summary>Call this before destroying a SceneLightSource at runtime.</summary>
    public void UnregisterLightSource(SceneLightSource source) {
        _sceneLights.Remove(source);
    }

    // ===========================================================
    //  Full Reveal Pass
    // ===========================================================

    /// <summary>
    /// Clears frame visibility, reveals from the player, then chain-activates
    /// scene lights using distance checks between outer radii.
    ///
    /// Activation is PERMANENT - once a light is activated it stays on forever
    /// (isActivated is never reset to false after it becomes true).
    ///
    /// A light activates when:
    ///   distance(light, nearestActiveSource) less than or equal to (lightOuterRadius + sourceOuterRadius)
    /// i.e. when the two outer glow rings are touching or overlapping.
    /// </summary>
    void RunFullRevealPass(Vector2Int playerTile) {
        // Clear per-frame visibility (used only for texture upload, not for activation).
        for (int i = 0; i < _frameVisibility.Length; i++)
            _frameVisibility[i] = -1f;

        // Reveal from player.
        RevealFromPoint(playerTile, visibleRadius, diffusionWidth);

        // Build a list of currently active light positions and their outer radii.
        // Starts with just the player, grows as lights chain-activate.
        // Each entry is (worldPos, outerRadius).
        List<Vector4> activeSources = new List<Vector4>();
        // Snap player to tile center so the reveal is always symmetric.
        Vector2 playerWorld = TileToWorld(playerTile);
        float playerOuter = (visibleRadius + diffusionWidth) * tileSize;
        activeSources.Add(new Vector4(playerWorld.x, playerWorld.y, playerOuter, 0));

        // Chain-reaction loop. Keep iterating until a full pass activates nothing new.
        // Already-activated lights are never re-evaluated, so this converges quickly.
        bool anyNew = true;
        while (anyNew) {
            anyNew = false;
            foreach (SceneLightSource light in _sceneLights) {
                // Permanently activated lights are skipped - they already contribute.
                if (light.isActivated) continue;

                float lightOuter = (light.radius + light.diffusionWidth) * tileSize;
                Vector2Int lightTile = WorldToTile(light.transform.position);
                Vector2 lightCenter = TileToWorld(lightTile);

                bool triggered = false;
                foreach (Vector4 src in activeSources) {
                    float dx = lightCenter.x - src.x;
                    float dy = lightCenter.y - src.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= lightOuter + src.z) { triggered = true; break; }
                }

                if (!triggered) continue;

                light.isActivated = true;
                anyNew = true;
                RevealFromPoint(lightTile, light.radius, light.diffusionWidth);
                activeSources.Add(new Vector4(lightCenter.x, lightCenter.y, lightOuter, 0));
            }
        }

        UploadFogTexture();
    }

    /// <summary>
    /// Casts LOS rays from originTile within radius+diffusion and writes results
    /// into _frameVisibility and _discoveryMap. Safe to call multiple times per
    /// pass - always keeps the highest visibility value per tile.
    /// </summary>
    void RevealFromPoint(Vector2Int originTile, float radius, float diffusion) {
        float outerRadius = radius + diffusion;
        int reach = Mathf.CeilToInt(outerRadius);

        int minX = Mathf.Max(0, originTile.x - reach);
        int maxX = Mathf.Min(_mapSize.x - 1, originTile.x + reach);
        int minY = Mathf.Max(0, originTile.y - reach);
        int maxY = Mathf.Min(_mapSize.y - 1, originTile.y + reach);

        for (int tx = minX; tx <= maxX; tx++) {
            for (int ty = minY; ty <= maxY; ty++) {
                float dx = tx - originTile.x;
                float dy = ty - originTile.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > outerRadius) continue;
                if (!HasLineOfSight(originTile, new Vector2Int(tx, ty))) continue;

                float vis = CalculateVisibility(dist, radius, diffusion);
                if (vis <= 0f) continue;

                int idx = Index(tx, ty);
                if (vis > _frameVisibility[idx]) _frameVisibility[idx] = vis;
                if (vis > _discoveryMap[idx]) _discoveryMap[idx] = vis;
            }
        }
    }

    float CalculateVisibility(float dist, float radius, float diffusion) {
        if (dist <= radius) return 1f;
        float t = (dist - radius) / Mathf.Max(diffusion, 0.0001f);
        return 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
    }

    // ===========================================================
    //  Fog Texture Upload
    // ===========================================================

    void UploadFogTexture() {
        int texW = _fogUploadTex.width;
        int texH = _fogUploadTex.height;

        for (int tx = 0; tx < _mapSize.x; tx++) {
            for (int ty = 0; ty < _mapSize.y; ty++) {
                int idx = Index(tx, ty);
                float disc = _discoveryMap[idx];
                float brightness = _tileExploredBrightness[idx];
                bool alwaysVis = _tileAlwaysVisible[idx];
                float frameVis = _frameVisibility[idx];

                float fogValue;
                if (disc <= 0f) fogValue = 0f;
                else if (alwaysVis) fogValue = 1f;
                else if (frameVis >= 0f) fogValue = Mathf.Lerp(brightness, 1f, frameVis);
                else fogValue = brightness * disc;

                int px = tx / textureDownscale;
                int py = ty / textureDownscale;
                if (px < texW && py < texH)
                    _fogPixels[py * texW + px] = new Color(fogValue, 0f, 0f, 1f);
            }
        }

        _fogUploadTex.SetPixels(_fogPixels);
        _fogUploadTex.Apply(false);
        Graphics.Blit(_fogUploadTex, _fogRT);
    }

    // ===========================================================
    //  Overlay Quad
    // ===========================================================

    void BuildOverlayQuad() {
        Mesh mesh = new Mesh();
        mesh.name = "FogOverlayQuad";
        mesh.vertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3( 0.5f, -0.5f, 0f),
            new Vector3(-0.5f,  0.5f, 0f),
            new Vector3( 0.5f,  0.5f, 0f)
        };
        mesh.uv = new Vector2[] {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f)
        };
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = fogMaterial;
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.receiveShadows = false;
        _meshRenderer.sortingLayerName = "Default";
        _meshRenderer.sortingOrder = 32000;

        UpdateQuadTransform();
    }

    void UpdateQuadTransform() {
        if (_cam == null) return;

        Vector3[] corners = new Vector3[4];
        _cam.CalculateFrustumCorners(
            new Rect(0, 0, 1, 1),
            _cam.nearClipPlane,
            Camera.MonoOrStereoscopicEye.Mono,
            corners
        );

        Vector3 camPos = _cam.transform.position;
        for (int i = 0; i < 4; i++)
            corners[i] = _cam.transform.TransformPoint(corners[i]);

        Vector2[] proj = new Vector2[4];
        for (int i = 0; i < 4; i++) {
            Vector3 dir = (corners[i] - camPos).normalized;
            float t = (Mathf.Abs(dir.z) > 0.0001f) ? (-camPos.z / dir.z) : 0f;
            Vector3 hit = camPos + dir * t;
            proj[i] = new Vector2(hit.x, hit.y);
        }

        float left = Mathf.Min(Mathf.Min(proj[0].x, proj[1].x), Mathf.Min(proj[2].x, proj[3].x));
        float right = Mathf.Max(Mathf.Max(proj[0].x, proj[1].x), Mathf.Max(proj[2].x, proj[3].x));
        float bottom = Mathf.Min(Mathf.Min(proj[0].y, proj[1].y), Mathf.Min(proj[2].y, proj[3].y));
        float top = Mathf.Max(Mathf.Max(proj[0].y, proj[1].y), Mathf.Max(proj[2].y, proj[3].y));

        float w = right - left;
        float h = top - bottom;

        transform.position = new Vector3(left + w * 0.5f, bottom + h * 0.5f, 0f);
        transform.localScale = new Vector3(w, h, 1f);
        transform.rotation = Quaternion.identity;

        if (fogMaterial != null) {
            fogMaterial.SetTexture(ShaderFogTex, _fogRT);
            fogMaterial.SetVector(ShaderFogWorldRect, new Vector4(
                _mapOrigin.x,
                _mapOrigin.y,
                _mapSize.x * tileSize,
                _mapSize.y * tileSize));
            fogMaterial.SetVector(ShaderQuadWorldRect, new Vector4(left, bottom, w, h));
        }
    }

    // ===========================================================
    //  Surface Reveal
    // ===========================================================

    /// <summary>
    /// Scans each column of the tilemap top-down to find the highest solid tile.
    /// Everything above that tile (plus surfaceRevealPaddingTiles) is marked as
    /// fully discovered so the player can see the sky and surface on start.
    /// </summary>
    void RevealSurface() {
        if (!revealSurfaceOnStart) return;

        int revealed = 0;
        for (int tx = 0; tx < _mapSize.x; tx++) {
            // Find the highest solid tile in this column.
            int surfaceY = -1;
            for (int ty = _mapSize.y - 1; ty >= 0; ty--) {
                if (_solidCache.Contains(new Vector2Int(tx, ty))) {
                    surfaceY = ty;
                    break;
                }
            }

            // Reveal all air above the surface (surfaceY+1 to top).
            int airStart = (surfaceY >= 0) ? surfaceY + 1 : 0;
            for (int ty = airStart; ty < _mapSize.y; ty++) {
                _discoveryMap[Index(tx, ty)] = 1f;
                revealed++;
            }

            // Optionally reveal the surface tile itself plus N tiles below it.
            if (surfaceY >= 0) {
                int solidEnd = Mathf.Max(0, surfaceY - surfaceRevealPaddingTiles);
                for (int ty = surfaceY; ty >= solidEnd; ty--) {
                    _discoveryMap[Index(tx, ty)] = 1f;
                    revealed++;
                }
            }
        }

        Debug.Log("[FogOfWar] Surface reveal: " + revealed + " tiles.");
        UploadFogTexture();
    }

    // ===========================================================
    //  Map Bounds
    // ===========================================================

    void DetectMapBounds() {
        if (solidTilemap == null) {
            Debug.LogError("[FogOfWar] solidTilemap not assigned.");
            enabled = false;
            return;
        }

        solidTilemap.CompressBounds();
        BoundsInt bounds = solidTilemap.cellBounds;
        _cellOffset = new Vector2Int(bounds.xMin, bounds.yMin);

        Vector3 firstCellCenter = solidTilemap.GetCellCenterWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        // _mapOrigin is the world-space bottom-left edge of tile (0,0).
        // Used for the shader FogWorldRect. CellCenterWorld - 0.5*tileSize = bottom-left corner.
        _mapOrigin = new Vector2(firstCellCenter.x - tileSize * 0.5f, firstCellCenter.y - tileSize * 0.5f);
        _mapSize = new Vector2Int(bounds.size.x, bounds.size.y);

        Debug.Log("[FogOfWar] Map: cellOffset=" + _cellOffset +
                  " firstCellCenter=" + firstCellCenter +
                  " mapOrigin(bottom-left)=" + _mapOrigin +
                  " size=" + _mapSize + " tileSize=" + tileSize);
    }

    // ===========================================================
    //  Solid Cache
    // ===========================================================

    public void RebuildSolidCache() {
        _solidCache.Clear();
        if (solidTilemap == null) return;
        BoundsInt bounds = solidTilemap.cellBounds;
        foreach (Vector3Int cell in bounds.allPositionsWithin) {
            if (!solidTilemap.HasTile(cell)) continue;
            _solidCache.Add(new Vector2Int(cell.x - _cellOffset.x, cell.y - _cellOffset.y));
        }
        Debug.Log("[FogOfWar] Solid cache: " + _solidCache.Count + " tiles.");
    }

    public void RemoveTileFromCache(Vector2Int tile) { _solidCache.Remove(tile); }
    public void AddTileToCache(Vector2Int tile) { _solidCache.Add(tile); }

    // ===========================================================
    //  Array and Texture Init
    // ===========================================================

    void InitArrays() {
        int total = _mapSize.x * _mapSize.y;
        _discoveryMap = new float[total];
        _tileExploredBrightness = new float[total];
        _tileAlwaysVisible = new bool[total];
        _frameVisibility = new float[total];

        for (int i = 0; i < total; i++) {
            _tileExploredBrightness[i] = exploredBrightness;
            _frameVisibility[i] = -1f;
        }

        if (tileSettings != null) ApplyTileSettings();
    }

    void ApplyTileSettings() {
        BoundsInt bounds = solidTilemap.cellBounds;
        foreach (Vector3Int cell in bounds.allPositionsWithin) {
            TileBase tb = solidTilemap.GetTile(cell);
            if (tb == null) continue;
            int tx = cell.x - _cellOffset.x;
            int ty = cell.y - _cellOffset.y;
            if (!InBounds(new Vector2Int(tx, ty))) continue;
            int idx = Index(tx, ty);
            _tileExploredBrightness[idx] = tileSettings.GetExploredBrightness(tb, exploredBrightness);
            _tileAlwaysVisible[idx] = tileSettings.IsAlwaysVisible(tb);
        }

        if (additionalTilemaps == null) return;
        foreach (Tilemap tm in additionalTilemaps) {
            if (tm == null) continue;
            tm.CompressBounds();
            foreach (Vector3Int cell in tm.cellBounds.allPositionsWithin) {
                TileBase tb = tm.GetTile(cell);
                if (tb == null) continue;
                int tx = cell.x - _cellOffset.x;
                int ty = cell.y - _cellOffset.y;
                if (!InBounds(new Vector2Int(tx, ty))) continue;
                int idx = Index(tx, ty);
                float b = tileSettings.GetExploredBrightness(tb, exploredBrightness);
                _tileExploredBrightness[idx] = Mathf.Min(_tileExploredBrightness[idx], b);
            }
        }
    }

    void InitFogTexture() {
        int texW = Mathf.Max(1, Mathf.CeilToInt((float)_mapSize.x / textureDownscale));
        int texH = Mathf.Max(1, Mathf.CeilToInt((float)_mapSize.y / textureDownscale));

        _fogRT = new RenderTexture(texW, texH, 0, RenderTextureFormat.R8);
        _fogRT.filterMode = FilterMode.Bilinear;
        _fogRT.wrapMode = TextureWrapMode.Clamp;
        _fogRT.name = "FogOfWarRT";
        _fogRT.Create();

        _fogUploadTex = new Texture2D(texW, texH, TextureFormat.R8, false);
        _fogUploadTex.filterMode = FilterMode.Bilinear;
        _fogUploadTex.wrapMode = TextureWrapMode.Clamp;

        _fogPixels = new Color[texW * texH];
        for (int i = 0; i < _fogPixels.Length; i++)
            _fogPixels[i] = Color.black;

        _fogUploadTex.SetPixels(_fogPixels);
        _fogUploadTex.Apply(false);
        Graphics.Blit(_fogUploadTex, _fogRT);
    }

    // ===========================================================
    //  Line of Sight
    // ===========================================================

    bool HasLineOfSight(Vector2Int from, Vector2Int to) {
        GetBresenhamLine(from, to, _lineBuffer);
        for (int i = 1; i < _lineBuffer.Count - 1; i++)
            if (_solidCache.Contains(_lineBuffer[i])) return false;
        return true;
    }

    void GetBresenhamLine(Vector2Int from, Vector2Int to, List<Vector2Int> result) {
        result.Clear();
        int x = from.x, y = from.y;
        int dx = Mathf.Abs(to.x - from.x), dy = Mathf.Abs(to.y - from.y);
        int sx = from.x < to.x ? 1 : -1;
        int sy = from.y < to.y ? 1 : -1;
        int err = dx - dy;
        while (true) {
            result.Add(new Vector2Int(x, y));
            if (x == to.x && y == to.y) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }
    }

    // ===========================================================
    //  Coordinate Utilities
    // ===========================================================

    public Vector2Int WorldToTile(Vector3 worldPos) {
        Vector3Int cell = solidTilemap.WorldToCell(worldPos);
        return new Vector2Int(cell.x - _cellOffset.x, cell.y - _cellOffset.y);
    }

    int Index(int x, int y) => y * _mapSize.x + x;
    bool InBounds(Vector2Int t) => t.x >= 0 && t.y >= 0 && t.x < _mapSize.x && t.y < _mapSize.y;

    // Returns the world-space center of a tile.
    Vector2 TileToWorld(Vector2Int tile) {
        Vector3 center = solidTilemap.GetCellCenterWorld(
            new Vector3Int(tile.x + _cellOffset.x, tile.y + _cellOffset.y, 0));
        return new Vector2(center.x, center.y);
    }

    // ===========================================================
    //  Public API
    // ===========================================================

    /// <summary>
    /// Call from your drill script after removing a tile.
    /// USAGE: fogManager.OnTileBroken(tilemap.WorldToCell(hitPosition));
    /// </summary>
    public void OnTileBroken(Vector3Int cellPos) {
        RemoveTileFromCache(new Vector2Int(cellPos.x - _cellOffset.x, cellPos.y - _cellOffset.y));
        if (player != null) {
            RunFullRevealPass(WorldToTile(player.position));
            _lastPlayerTile = new Vector2Int(int.MinValue, int.MinValue);
        }
    }

    /// <summary>Call after placing a tile (e.g. player builds a wall).</summary>
    public void OnTilePlaced(Vector3Int cellPos) {
        AddTileToCache(new Vector2Int(cellPos.x - _cellOffset.x, cellPos.y - _cellOffset.y));
        if (player != null) {
            RunFullRevealPass(WorldToTile(player.position));
            _lastPlayerTile = new Vector2Int(int.MinValue, int.MinValue);
        }
    }

    public float GetDiscovery(Vector3 worldPos) {
        Vector2Int t = WorldToTile(worldPos);
        return InBounds(t) ? _discoveryMap[Index(t.x, t.y)] : 0f;
    }

    public float GetDiscoveryAtTile(Vector2Int tile) {
        return InBounds(tile) ? _discoveryMap[Index(tile.x, tile.y)] : 0f;
    }

    public void RevealTile(Vector2Int tile, float amount = 1f) {
        if (!InBounds(tile)) return;
        int idx = Index(tile.x, tile.y);
        _discoveryMap[idx] = Mathf.Max(_discoveryMap[idx], Mathf.Clamp01(amount));
    }

    public byte[] SerializeDiscoveryMap() {
        byte[] data = new byte[_discoveryMap.Length];
        for (int i = 0; i < _discoveryMap.Length; i++)
            data[i] = (byte)Mathf.RoundToInt(_discoveryMap[i] * 255f);
        return data;
    }

    public void DeserializeDiscoveryMap(byte[] data) {
        if (data == null || data.Length != _discoveryMap.Length) {
            Debug.LogWarning("[FogOfWar] Discovery map size mismatch on load.");
            return;
        }
        for (int i = 0; i < data.Length; i++)
            _discoveryMap[i] = data[i] / 255f;
        _lastPlayerTile = new Vector2Int(int.MinValue, int.MinValue);
    }

    // ===========================================================
    //  Gizmos
    // ===========================================================

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        if (player == null) return;

        // Player light radii.
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        DrawCircle(player.position, visibleRadius * tileSize);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        DrawCircle(player.position, (visibleRadius + diffusionWidth) * tileSize);

        // Draw small crosses at tile centers near the player to verify fog alignment.
        // Each cross sits exactly at where a fog texel center should be.
        if (Application.isPlaying && solidTilemap != null) {
            Vector2Int pt = WorldToTile(player.position);
            Gizmos.color = new Color(0f, 1f, 1f, 0.6f);
            float s = tileSize * 0.15f;
            for (int dx = -3; dx <= 3; dx++) {
                for (int dy = -3; dy <= 3; dy++) {
                    Vector2Int t = new Vector2Int(pt.x + dx, pt.y + dy);
                    if (!InBounds(t)) continue;
                    Vector2 c = TileToWorld(t);
                    Gizmos.DrawLine(new Vector3(c.x - s, c.y, 0), new Vector3(c.x + s, c.y, 0));
                    Gizmos.DrawLine(new Vector3(c.x, c.y - s, 0), new Vector3(c.x, c.y + s, 0));
                }
            }
        }

        // Scene lights.
        SceneLightSource[] lights = UnityEngine.Object.FindObjectsByType<SceneLightSource>(FindObjectsSortMode.None);
        foreach (SceneLightSource light in lights) {
            if (light == null) continue;
            bool on = light.isActivated || light.alwaysActive;
            Gizmos.color = on ? new Color(1f, 0.8f, 0f, 0.5f) : new Color(0.4f, 0.4f, 0.4f, 0.4f);
            DrawCircle(light.transform.position, light.radius * tileSize);
            Gizmos.color = on ? new Color(1f, 0.8f, 0f, 0.2f) : new Color(0.4f, 0.4f, 0.4f, 0.2f);
            DrawCircle(light.transform.position, (light.radius + light.diffusionWidth) * tileSize);
        }
    }

    void DrawCircle(Vector3 c, float r) {
        float step = (360f / 40) * Mathf.Deg2Rad;
        Vector3 prev = c + new Vector3(r, 0f, 0f);
        for (int i = 1; i <= 40; i++) {
            float a = i * step;
            Vector3 next = c + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}