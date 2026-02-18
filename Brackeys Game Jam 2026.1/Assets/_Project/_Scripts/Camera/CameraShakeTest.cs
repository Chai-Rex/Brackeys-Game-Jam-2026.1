using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Development-only test script for the CameraHandler trauma / screen shake system.
///
/// Setup
/// -----
///   1. Add this component to any GameObject in the scene (e.g. a "DebugTools" object).
///   2. Assign the CameraHandler and SceneContainerSO references in the Inspector.
///   3. Enter Play mode and press the TestDamage input action to fire shakes.
///
/// Each press cycles through Light -> Medium -> Heavy -> Explosion trauma amounts
/// so you can feel the full range in one playtest session without touching code.
/// You can also assign a fixed trauma amount in the Inspector if you prefer.
/// </summary>
public class CameraShakeTest : MonoBehaviour {

    [Header("References")]
    [SerializeField] private CameraHandler _cameraHandler;
    [SerializeField] private SceneContainerSO _sceneContainer;

    [Header("Shake Settings")]
    [Tooltip("If true, cycles through the preset trauma amounts below with each press. " +
             "If false, always uses FixedTraumaAmount.")]
    [SerializeField] private bool _cyclePresets = true;
    [SerializeField] private float _fixedTraumaAmount = 0.4f;

    [Header("Cycle Presets")]
    [SerializeField] private float _lightTrauma = 0.2f;
    [SerializeField] private float _mediumTrauma = 0.4f;
    [SerializeField] private float _heavyTrauma = 0.6f;
    [SerializeField] private float _explosionTrauma = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool _logShakes = true;

    // ==== Private =====================================

    private InputManager _inputManager;
    private int _presetIndex = 0;

    private readonly string[] _presetNames = { "Light", "Medium", "Heavy", "Explosion" };
    private float[] _presetValues;

    ////////////////////////////////////////////////////////////
    #region Unity Lifecycle
    ////////////////////////////////////////////////////////////

    private void Awake() {
        _presetValues = new float[] { _lightTrauma, _mediumTrauma, _heavyTrauma, _explosionTrauma };
    }

    private void Start() {
        // Resolve InputManager through SceneContainer if not set up yet
        if (_sceneContainer != null) {
            _inputManager = _sceneContainer.GetManager<InputManager>();
        }

        if (_inputManager == null) {
            Debug.LogError("[CameraShakeTest] Could not find InputManager. " +
                           "Make sure SceneContainer is assigned and InputManager is initialized.");
            return;
        }

        if (_inputManager._TestDamageAction == null) {
            Debug.LogError("[CameraShakeTest] _TestDamageAction is null. " +
                           "Check that 'TestDamage' action exists in your Input Action Asset.");
            return;
        }

        _inputManager._TestDamageAction.performed += OnTestDamagePressed;
        Log("Subscribed to TestDamage input action. Press it in Play mode to test screen shake.");
    }

    private void OnDestroy() {
        if (_inputManager?._TestDamageAction != null) {
            _inputManager._TestDamageAction.performed -= OnTestDamagePressed;
        }
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Input Callback
    ////////////////////////////////////////////////////////////

    private void OnTestDamagePressed(InputAction.CallbackContext ctx) {
        if (_cameraHandler == null) {
            Debug.LogWarning("[CameraShakeTest] CameraHandler is not assigned.");
            return;
        }

        float trauma;
        string label;

        if (_cyclePresets) {
            // Refresh array in case Inspector values changed at runtime
            _presetValues[0] = _lightTrauma;
            _presetValues[1] = _mediumTrauma;
            _presetValues[2] = _heavyTrauma;
            _presetValues[3] = _explosionTrauma;

            trauma = _presetValues[_presetIndex];
            label = _presetNames[_presetIndex];
            _presetIndex = (_presetIndex + 1) % _presetValues.Length;
        } else {
            trauma = _fixedTraumaAmount;
            label = $"Fixed ({trauma:F2})";
        }

        _cameraHandler.AddTrauma(trauma);
        Log($"Added trauma: {label} ({trauma:F2}) | Current trauma: {_cameraHandler.GetTrauma():F2}");
    }

    #endregion

    ////////////////////////////////////////////////////////////
    #region Logging
    ////////////////////////////////////////////////////////////

    private void Log(string message) {
        if (_logShakes) Debug.Log($"[CameraShakeTest] {message}");
    }

    #endregion

#if UNITY_EDITOR
    ////////////////////////////////////////////////////////////
    #region Editor GUI  (shows current trauma level in Scene view)
    ////////////////////////////////////////////////////////////

    private void OnDrawGizmosSelected() {
        if (_cameraHandler == null) return;

        // Draw a small bar above this object showing current trauma level
        float trauma = _cameraHandler.GetTrauma();
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        float barWidth = 2f;

        // Background
        UnityEditor.Handles.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        UnityEditor.Handles.DrawLine(origin + Vector3.left * (barWidth * 0.5f),
                                     origin + Vector3.right * (barWidth * 0.5f));

        // Trauma fill (red -> yellow -> green based on amount)
        Color fillColor = Color.Lerp(Color.green, Color.red, trauma);
        UnityEditor.Handles.color = fillColor;
        UnityEditor.Handles.DrawLine(origin + Vector3.left * (barWidth * 0.5f),
                                     origin + Vector3.left * (barWidth * 0.5f) +
                                     Vector3.right * (barWidth * trauma));

        // Label
        UnityEditor.Handles.Label(origin + Vector3.up * 0.2f,
                                  $"Trauma: {trauma:F2}  [Press {_presetNames[_presetIndex % _presetNames.Length]} next]");
    }

    #endregion
#endif
}