using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives FogOfWarManager's visible radius and diffusion width from
/// equipped items and temporary buffs.
///
/// SETUP:
///   1. Attach to your Player GameObject.
///   2. Assign the FogOfWarManager reference.
///   3. Call AddLightSource / RemoveLightSource from your inventory system.
///   4. Call AddBuff for temporary light increases (potions, abilities, etc.)
///
/// USAGE EXAMPLES:
///   light.AddLightSource("torch",         radius: 6f, diffusion: 2.5f);
///   light.AddLightSource("mining_helmet", radius: 4f, diffusion: 1.5f);
///   light.RemoveLightSource("torch");
///   light.AddBuff(radiusBonus: 8f, duration: 30f);
/// </summary>
public class PlayerLightSource : MonoBehaviour {
    [Header("References")]
    public FogOfWarManager fogManager;

    [Header("Base Light (no equipment)")]
    [Tooltip("Minimum light radius the player always has even with no items.")]
    public float baseLightRadius = 3f;

    [Tooltip("Minimum diffusion width.")]
    public float baseDiffusionWidth = 2f;

    [Header("Read Only - Current Totals")]
    [SerializeField] private float _currentRadius;
    [SerializeField] private float _currentDiffusion;

    // ===========================================================
    //  Internal State
    // ===========================================================

    private Dictionary<string, LightSourceData> _sources = new Dictionary<string, LightSourceData>();
    private List<LightBuff> _buffs = new List<LightBuff>();

    // ===========================================================
    //  Unity Lifecycle
    // ===========================================================

    void Start() {
        if (fogManager == null)
            fogManager = FindObjectOfType<FogOfWarManager>();
        Recalculate();
    }

    void Update() {
        bool dirty = false;
        for (int i = _buffs.Count - 1; i >= 0; i--) {
            _buffs[i].remainingTime -= Time.deltaTime;
            if (_buffs[i].remainingTime <= 0f) {
                _buffs.RemoveAt(i);
                dirty = true;
            }
        }
        if (dirty) Recalculate();
    }

    // ===========================================================
    //  Public API
    // ===========================================================

    /// <summary>
    /// Register a persistent light source (e.g. equipping a torch).
    /// Replaces any existing source with the same key.
    /// </summary>
    public void AddLightSource(string key, float radius, float diffusion = 0f) {
        _sources[key] = new LightSourceData(radius, diffusion);
        Recalculate();
    }

    /// <summary>
    /// Remove a persistent light source (e.g. unequipping an item).
    /// </summary>
    public void RemoveLightSource(string key) {
        if (_sources.Remove(key))
            Recalculate();
    }

    /// <summary>
    /// Remove all registered persistent light sources.
    /// </summary>
    public void ClearLightSources() {
        _sources.Clear();
        Recalculate();
    }

    /// <summary>
    /// Apply a temporary light radius increase for a number of seconds.
    /// </summary>
    public void AddBuff(float radiusBonus, float duration, float diffusionBonus = 0f) {
        _buffs.Add(new LightBuff(radiusBonus, diffusionBonus, duration));
        Recalculate();
    }

    public float CurrentRadius => _currentRadius;
    public float CurrentDiffusion => _currentDiffusion;

    // ===========================================================
    //  Internal Recalculation
    // ===========================================================

    void Recalculate() {
        float r = baseLightRadius;
        float d = baseDiffusionWidth;

        foreach (LightSourceData src in _sources.Values) {
            r += src.radius;
            d += src.diffusion;
        }

        foreach (LightBuff buf in _buffs) {
            r += buf.radiusBonus;
            d += buf.diffusionBonus;
        }

        _currentRadius = Mathf.Max(0f, r);
        _currentDiffusion = Mathf.Max(0f, d);

        if (fogManager != null) {
            fogManager.visibleRadius = _currentRadius;
            fogManager.diffusionWidth = _currentDiffusion;
        }
    }

    // ===========================================================
    //  Data Types
    // ===========================================================

    private struct LightSourceData {
        public float radius;
        public float diffusion;

        public LightSourceData(float r, float d) {
            radius = r;
            diffusion = d;
        }
    }

    private class LightBuff {
        public float radiusBonus;
        public float diffusionBonus;
        public float remainingTime;

        public LightBuff(float r, float d, float t) {
            radiusBonus = r;
            diffusionBonus = d;
            remainingTime = t;
        }
    }
}