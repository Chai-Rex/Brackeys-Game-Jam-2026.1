using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Complete fog of war system using a simple quad overlay.
/// 
/// NO URP Renderer Features needed.
/// NO fullscreen pass needed.
/// NO shader world-position reconstruction.
/// 
/// HOW IT WORKS:
///   A black quad mesh is created at runtime and parented to the camera.
///   The quad always fills the screen. The fog material on the quad uses
///   a simple UV that matches world space, sampling a RenderTexture that
///   contains the fog discovery data.
/// 
///   The fog material just needs to know the quad's world-space position
///   and size, which are passed as simple uniforms every frame.
/// 
/// SETUP:
///   1. Create an empty GameObject, name it FogOfWar.
///   2. Attach this script.
///   3. Assign solidTilemap (your terrain tilemap with the most tiles).
///   4. Assign player (your player Transform).
///   5. Create a Material using the FogOfWar.shader, assign to fogMaterial.
///   6. That is all. No Renderer Features, no camera settings.
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

    [Tooltip("Radius in tiles where visibility is 100%.")]
    public float visibleRadius = 8f;

    [Tooltip("Extra tiles beyond visibleRadius where light fades to 0.")]
    public float diffusionWidth = 3f;

    [Header("Performance")]
    [Range(1, 4)]
    public int textureDownscale = 1;

    // ===========================================================
    //  Private
    // ===========================================================

    private float[] _discoveryMap;
    private float[] _tileExploredBrightness;
    private bool[] _tileAlwaysVisible;
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

    private Camera _cam;
    private MeshRenderer _meshRenderer;

    private static readonly int ShaderFogTex = Shader.PropertyToID("_FogTex");
    private static readonly int ShaderFogWorldRect = Shader.PropertyToID("_FogWorldRect");

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
    }

    void OnDestroy() {
        if (_fogRT != null) { _fogRT.Release(); _fogRT = null; }
    }

    void LateUpdate() {
        if (player == null || _cam == null) return;

        // Move the quad to always cover the camera view.
        UpdateQuadTransform();

        Vector2Int playerTile = WorldToTile(player.position);
        if (playerTile == _lastPlayerTile) return;
        _lastPlayerTile = playerTile;

        ComputeVisibilityAndReveal(playerTile);
        UploadFogTexture(playerTile);
    }

    // ===========================================================
    //  Overlay Quad
    // ===========================================================

    void BuildOverlayQuad() {
        // Build a simple unit quad mesh.
        Mesh mesh = new Mesh();
        mesh.name = "FogOverlayQuad";

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3( 0.5f, -0.5f, 0f),
            new Vector3(-0.5f,  0.5f, 0f),
            new Vector3( 0.5f,  0.5f, 0f)
        };

        mesh.uv = new Vector2[]
        {
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

        // Put the fog quad above all tilemaps in the sorting order.
        // "Default" sorting layer, very high order so it draws last (on top of tiles).
        // If your tilemaps use a different sorting layer, change "Default" to match,
        // or create a dedicated "Fog" sorting layer above your tile layers.
        _meshRenderer.sortingLayerName = "Default";
        _meshRenderer.sortingOrder = 32000;

        UpdateQuadTransform();
    }

    /// <summary>
    /// Sizes and positions the quad to exactly cover the camera view
    /// by projecting the camera frustum corners onto the world z=0 plane.
    /// This eliminates parallax and works correctly with perspective cameras.
    /// </summary>
    void UpdateQuadTransform() {
        if (_cam == null) return;

        // Get the four frustum corners at the near clip plane in view space.
        // Order: [0]=BL, [1]=TL, [2]=TR, [3]=BR
        Vector3[] corners = new Vector3[4];
        _cam.CalculateFrustumCorners(
            new Rect(0, 0, 1, 1),
            _cam.nearClipPlane,
            Camera.MonoOrStereoscopicEye.Mono,
            corners
        );

        // Transform from view space to world space.
        Vector3 camPos = _cam.transform.position;
        for (int i = 0; i < 4; i++)
            corners[i] = _cam.transform.TransformPoint(corners[i]);

        // Project each corner onto the world z=0 plane along the ray from the camera.
        // Ray: P(t) = camPos + t * dir, solve for t where P.z = 0 => t = -camPos.z / dir.z
        Vector2[] projected = new Vector2[4];
        for (int i = 0; i < 4; i++) {
            Vector3 dir = (corners[i] - camPos).normalized;
            float t = (Mathf.Abs(dir.z) > 0.0001f) ? (-camPos.z / dir.z) : 0f;
            Vector3 hit = camPos + dir * t;
            projected[i] = new Vector2(hit.x, hit.y);
        }

        // Find the axis-aligned bounding rect of the projected corners.
        // corners order: [0]=BL, [1]=TL, [2]=TR, [3]=BR
        float left = Mathf.Min(Mathf.Min(projected[0].x, projected[1].x), Mathf.Min(projected[2].x, projected[3].x));
        float right = Mathf.Max(Mathf.Max(projected[0].x, projected[1].x), Mathf.Max(projected[2].x, projected[3].x));
        float bottom = Mathf.Min(Mathf.Min(projected[0].y, projected[1].y), Mathf.Min(projected[2].y, projected[3].y));
        float top = Mathf.Max(Mathf.Max(projected[0].y, projected[1].y), Mathf.Max(projected[2].y, projected[3].y));

        float width = right - left;
        float height = top - bottom;

        // Position at z=0 (same plane as the grid, no parallax).
        transform.position = new Vector3(left + width * 0.5f, bottom + height * 0.5f, 0f);
        transform.localScale = new Vector3(width, height, 1f);
        transform.rotation = Quaternion.identity;

        // Pass the quad's world rect to the shader.
        if (fogMaterial != null) {
            fogMaterial.SetTexture(ShaderFogTex, _fogRT);
            fogMaterial.SetVector(ShaderFogWorldRect, new Vector4(
                _mapOrigin.x,
                _mapOrigin.y,
                _mapSize.x * tileSize,
                _mapSize.y * tileSize
            ));
            fogMaterial.SetVector("_QuadWorldRect", new Vector4(
                left, bottom, width, height
            ));
        }
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

        Vector3 originWorld = solidTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        _mapOrigin = new Vector2(originWorld.x, originWorld.y);
        _mapSize = new Vector2Int(bounds.size.x, bounds.size.y);

        Debug.Log("[FogOfWar] Map detected: origin=" + _mapOrigin +
                  " size=" + _mapSize + " (" + _mapSize.x * _mapSize.y + " tiles)");
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
            int tx = cell.x - _cellOffset.x;
            int ty = cell.y - _cellOffset.y;
            _solidCache.Add(new Vector2Int(tx, ty));
        }
        Debug.Log("[FogOfWar] Solid cache built: " + _solidCache.Count + " solid tiles.");
    }

    public void RemoveTileFromCache(Vector2Int tile) { _solidCache.Remove(tile); }
    public void AddTileToCache(Vector2Int tile) { _solidCache.Add(tile); }

    // ===========================================================
    //  Arrays
    // ===========================================================

    void InitArrays() {
        int total = _mapSize.x * _mapSize.y;
        _discoveryMap = new float[total];
        _tileExploredBrightness = new float[total];
        _tileAlwaysVisible = new bool[total];
        _frameVisibility = new float[total];

        for (int i = 0; i < total; i++)
            _tileExploredBrightness[i] = exploredBrightness;

        if (tileSettings != null)
            ApplyTileSettings();
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

    // ===========================================================
    //  Fog Texture
    // ===========================================================

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
    //  Visibility + Reveal
    // ===========================================================

    void ComputeVisibilityAndReveal(Vector2Int playerTile) {
        float outerRadius = visibleRadius + diffusionWidth;
        int reach = Mathf.CeilToInt(outerRadius);

        int minX = Mathf.Max(0, playerTile.x - reach);
        int maxX = Mathf.Min(_mapSize.x - 1, playerTile.x + reach);
        int minY = Mathf.Max(0, playerTile.y - reach);
        int maxY = Mathf.Min(_mapSize.y - 1, playerTile.y + reach);

        // Clear frame visibility cache for this region.
        for (int tx = minX; tx <= maxX; tx++)
            for (int ty = minY; ty <= maxY; ty++)
                _frameVisibility[Index(tx, ty)] = -1f;

        for (int tx = minX; tx <= maxX; tx++) {
            for (int ty = minY; ty <= maxY; ty++) {
                float dx = tx - playerTile.x;
                float dy = ty - playerTile.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > outerRadius) continue;

                if (!HasLineOfSight(playerTile, new Vector2Int(tx, ty))) continue;

                float vis = CalculateVisibility(dist);
                if (vis <= 0f) continue;

                int idx = Index(tx, ty);
                _frameVisibility[idx] = vis;
                if (vis > _discoveryMap[idx])
                    _discoveryMap[idx] = vis;
            }
        }
    }

    float CalculateVisibility(float dist) {
        if (dist <= visibleRadius) return 1f;
        float t = (dist - visibleRadius) / Mathf.Max(diffusionWidth, 0.0001f);
        return 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
    }

    // ===========================================================
    //  Texture Upload
    // ===========================================================

    void UploadFogTexture(Vector2Int playerTile) {
        float outerRadius = visibleRadius + diffusionWidth;
        int reach = Mathf.CeilToInt(outerRadius) + 1;

        int texW = _fogUploadTex.width;
        int texH = _fogUploadTex.height;

        int minTX = Mathf.Max(0, playerTile.x - reach);
        int maxTX = Mathf.Min(_mapSize.x - 1, playerTile.x + reach);
        int minTY = Mathf.Max(0, playerTile.y - reach);
        int maxTY = Mathf.Min(_mapSize.y - 1, playerTile.y + reach);

        for (int tx = minTX; tx <= maxTX; tx++) {
            for (int ty = minTY; ty <= maxTY; ty++) {
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

        int rectX = minTX / textureDownscale;
        int rectY = minTY / textureDownscale;
        int rectW = Mathf.Clamp((maxTX / textureDownscale) - rectX + 1, 1, texW - rectX);
        int rectH = Mathf.Clamp((maxTY / textureDownscale) - rectY + 1, 1, texH - rectY);

        Color[] sub = new Color[rectW * rectH];
        for (int sy = 0; sy < rectH; sy++)
            for (int sx = 0; sx < rectW; sx++)
                sub[sy * rectW + sx] = _fogPixels[(rectY + sy) * texW + (rectX + sx)];

        _fogUploadTex.SetPixels(rectX, rectY, rectW, rectH, sub);
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
        return new Vector2Int(
            Mathf.FloorToInt((worldPos.x - _mapOrigin.x) / tileSize),
            Mathf.FloorToInt((worldPos.y - _mapOrigin.y) / tileSize)
        );
    }

    int Index(int x, int y) => y * _mapSize.x + x;
    bool InBounds(Vector2Int t) => t.x >= 0 && t.y >= 0 && t.x < _mapSize.x && t.y < _mapSize.y;

    // ===========================================================
    //  Public API
    // ===========================================================

    public float GetDiscovery(Vector3 worldPos) {
        Vector2Int t = WorldToTile(worldPos);
        if (!InBounds(t)) return 0f;
        return _discoveryMap[Index(t.x, t.y)];
    }

    public float GetDiscoveryAtTile(Vector2Int tile) {
        if (!InBounds(tile)) return 0f;
        return _discoveryMap[Index(tile.x, tile.y)];
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
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        DrawCircle(player.position, visibleRadius * tileSize);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        DrawCircle(player.position, (visibleRadius + diffusionWidth) * tileSize);
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