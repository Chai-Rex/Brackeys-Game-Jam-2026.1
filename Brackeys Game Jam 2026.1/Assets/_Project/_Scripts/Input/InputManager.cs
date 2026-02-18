using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputManager", menuName = "ScriptableObjects/Managers/InputManager")]
public class InputManager : ScriptableObject, IInitializable, ICleanable, IPersistentManager {

    public event UnityAction<string> _OnControlsChanged;

    // Player Actions
    public InputAction _PlayerMoveAction { get; private set; }
    public InputAction _PlayerDrillAction { get; private set; }
    public InputAction _PlayerInteractAction { get; private set; }
    public InputAction _PlayerCrouchAction { get; private set; }
    public InputAction _PlayerJumpAction { get; private set; }
    public InputAction _PlayerDodgeAction { get; private set; }
    public InputAction _PlayerPauseAction { get; private set; }
    public InputAction _TestDamageAction { get; private set; }

    // UI Actions
    public InputAction _UINavigateAction { get; private set; }
    public InputAction _UIResumeAction { get; private set; }
    public InputAction _UISubmitAction { get; private set; }

    // Dialogue Actions
    public InputAction _DialogueNavigateAction { get; private set; }
    public InputAction _DialogueContinueAction { get; private set; }
    public InputAction _DialoguePauseAction { get; private set; }

    // Death Actions
    public InputAction _DeathRespawnAction { get; private set; }

    public string _ManagerName => GetType().Name;

    ////////////////////////////////////////////////////////////

    [Header("References")]
    [SerializeField] private PlayerInput _iPlayerInputPrefab;

    PlayerInput _playerInput;

    private InputActionAsset _inputActionsAsset;
    private Gamepad _gamepad;

    private CancellationTokenSource _rumbleLoopCts;

    // Blended rumble state
    private float _targetLowFreq;
    private float _targetHighFreq;

    ////////////////////////////////////////////////////////////
    /// <summary>
    /// Initializes the input manager by instantiating PlayerInput,
    /// binding events, and caching actions. Safe to call once.
    /// </summary>
    ////////////////////////////////////////////////////////////
    public async Task Initialize() {


        _playerInput = Instantiate(_iPlayerInputPrefab);
        _playerInput.onControlsChanged += PlayerInput_onControlsChanged;

        _inputActionsAsset = _playerInput.actions;

        GetInputActions();
        PlayerInput_onControlsChanged(_playerInput);

        StartRumbleLoop();

        Log("InputManager initialized.");
        await Task.Yield();
    }

    ////////////////////////////////////////////////////////////
    /// <summary>
    /// Cleans up runtime bindings and stops rumble safely.
    /// </summary>
    ////////////////////////////////////////////////////////////
    public void CleanUp() {

        if (_playerInput != null)
            _playerInput.onControlsChanged -= PlayerInput_onControlsChanged;

        _rumbleLoopCts?.Cancel();
        StopRumble();

        Log("InputManager cleaned up.");
    }

    ////////////////////////////////////////////////////////////
    /// <summary>
    /// Responds to Unity control scheme changes and updates device references.
    /// </summary>
    ////////////////////////////////////////////////////////////
    private void PlayerInput_onControlsChanged(PlayerInput i_playerInput) {

        _gamepad = _playerInput.currentControlScheme == "Gamepad"
            ? Gamepad.current
            : null;

        _OnControlsChanged?.Invoke(_playerInput.currentControlScheme);

        Log($"Controls changed -> {_playerInput.currentControlScheme}");
    }

    ////////////////////////////////////////////////////////////
    /// <summary>
    /// Finds and enables all required input actions from the asset.
    /// </summary>
    ////////////////////////////////////////////////////////////
    private void GetInputActions() {

        _PlayerMoveAction = BuildAction("Move");
        _PlayerDrillAction = BuildAction("Drill");
        _PlayerInteractAction = BuildAction("Interact");
        _PlayerCrouchAction = BuildAction("Crouch");
        _PlayerJumpAction = BuildAction("Jump");
        _PlayerDodgeAction = BuildAction("Dodge");
        _PlayerPauseAction = BuildAction("PlayerPause");
        _TestDamageAction = BuildAction("TestDamage");

        _UINavigateAction = BuildAction("UINavigation");
        _UIResumeAction = BuildAction("Unpause");
        _UISubmitAction = BuildAction("Submit");

        _DialogueNavigateAction = BuildAction("DialogueNavigation");
        _DialogueContinueAction = BuildAction("Continue");
        _DialoguePauseAction = BuildAction("DialoguePause");

        _DeathRespawnAction = BuildAction("Respawn");
    }

    /// <summary>
    /// Helper for safely locating and enabling an InputAction.
    /// </summary>
    private InputAction BuildAction(string i_name) {

        var action = _inputActionsAsset?.FindAction(i_name, false);

        if (action != null)
            action.Enable();
        else
            Debug.LogWarning($"Action '{i_name}' not found.");

        return action;
    }

    ////////////////////////////////////////////////////////////
    /// Switches the current Unity action map 
    ////////////////////////////////////////////////////////////
    public void SetPlayerActionMap() => SwitchMap("Player");
    public void SetUIActionMap() => SwitchMap("UI");
    public void SetDialogueActionMap() => SwitchMap("Dialogue");
    public void SetDeathActionMap() => SwitchMap("Death");

    private void SwitchMap(string i_mapName) {

        if (_playerInput == null) return;
        if (_playerInput.currentActionMap?.name == i_mapName) return;

        _playerInput.SwitchCurrentActionMap(i_mapName);

        Log($"Action map -> {i_mapName}");
    }

    ////////////////////////////////////////////////////////////
    #region Rumble System
    ////////////////////////////////////////////////////////////

    /// <summary>
    /// Immediately sets rumble motor speeds.
    /// </summary>
    public void SetRumble(float i_lowFrequency, float i_highFrequency) {

        if (_gamepad == null) return;

        _targetLowFreq = Mathf.Clamp01(i_lowFrequency);
        _targetHighFreq = Mathf.Clamp01(i_highFrequency);
    }

    /// <summary>
    /// Stops all rumble output.
    /// </summary>
    public void StopRumble() {
        _targetLowFreq = 0f;
        _targetHighFreq = 0f;

        if (_gamepad != null && _gamepad.added)
            _gamepad.SetMotorSpeeds(0f, 0f);
    }

    /// <summary>
    /// Plays a rumble animation blended with existing vibration.
    /// </summary>
    public void RumbleDuration(AnimationCurve i_lowCurve,
                               AnimationCurve i_highCurve,
                               float i_duration,
                               float i_intensity = 1f) {

        if (_gamepad == null) return;

        _ = RumbleAnimation(i_lowCurve, i_highCurve, i_duration, i_intensity);
    }

    /// <summary>
    /// Core rumble animation that blends into target frequencies.
    /// </summary>
    private async Task RumbleAnimation(AnimationCurve i_lowCurve,
                                       AnimationCurve i_highCurve,
                                       float i_duration,
                                       float i_intensity) {

        float elapsed = 0f;
        i_intensity = Mathf.Clamp01(i_intensity);

        while (elapsed < i_duration) {

            if (_gamepad == null || !_gamepad.added)
                return;

            elapsed += Time.deltaTime;
            float t = elapsed / i_duration;

            _targetLowFreq = Mathf.Max(_targetLowFreq, i_lowCurve.Evaluate(t) * i_intensity);
            _targetHighFreq = Mathf.Max(_targetHighFreq, i_highCurve.Evaluate(t) * i_intensity);

            await Task.Yield();
        }
    }

    /// <summary>
    /// Background loop that applies blended rumble safely.
    /// </summary>
    private void StartRumbleLoop() {

        _rumbleLoopCts?.Cancel();
        _rumbleLoopCts = new CancellationTokenSource();

        _ = RumbleLoop(_rumbleLoopCts.Token);
    }

    private async Task RumbleLoop(CancellationToken token) {

        try {
            while (!token.IsCancellationRequested) {

                if (_gamepad != null && _gamepad.added) {
                    _gamepad.SetMotorSpeeds(_targetLowFreq, _targetHighFreq);
                }

                // Natural decay for smooth blending
                _targetLowFreq = Mathf.MoveTowards(_targetLowFreq, 0f, Time.deltaTime * 2f);
                _targetHighFreq = Mathf.MoveTowards(_targetHighFreq, 0f, Time.deltaTime * 2f);

                await Task.Yield();
            }
        } finally {
            StopRumble();
        }
    }

    ////////////////////////////////////////////////////////////
    // Rumble Presets
    ////////////////////////////////////////////////////////////

    /// <summary>Short UI feedback vibration.</summary>
    public void Rumble_UI_Tick() => SetRumble(0.1f, 0.1f);

    /// <summary>Impact-style vibration.</summary>
    public void Rumble_Impact() => SetRumble(0.6f, 0.8f);

    /// <summary>Damage-style vibration.</summary>
    public void Rumble_Damage() => SetRumble(0.9f, 0.3f);

    /// <summary>Explosion-style vibration.</summary>
    public void Rumble_Explosion() => SetRumble(1f, 1f);

    #endregion

    ////////////////////////////////////////////////////////////
    #region Debugging
    ////////////////////////////////////////////////////////////

    [Header("Debug")]
    [SerializeField] private bool _debugLogs = false;

    /// <summary>
    /// Logs debug messages when enabled.
    /// </summary>
    private void Log(string i_message) {
        if (_debugLogs)
            Debug.Log($"[InputManager] {i_message}");
    }

    #endregion
}
