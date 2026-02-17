using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input and updates blackboard with input state
/// Manages input buffering for jump
/// </summary>
public class PlayerInputHandler : MonoBehaviour {
    private InputManager _inputManager;
    private PlayerBlackboardHandler _blackboard;
    private PlayerStatsHandler _stats;

    private Coroutine _jumpBufferRoutine;

    public void Initialize(InputManager inputManager, PlayerBlackboardHandler blackboard, PlayerStatsHandler stats) {
        _inputManager = inputManager;
        _blackboard = blackboard;
        _stats = stats;

        SubscribeToInput();
    }

    private void OnDestroy() {
        UnsubscribeFromInput();
    }

    #region Input Subscription

    private void SubscribeToInput() {
        if (_inputManager == null) return;

        _inputManager._PlayerMoveAction.performed += OnMove;
        _inputManager._PlayerMoveAction.canceled += OnMove;

        _inputManager._PlayerJumpAction.started += OnJumpStarted;
        _inputManager._PlayerJumpAction.canceled += OnJumpCanceled;

        _inputManager._PlayerCrouchAction.started += OnCrouchStarted;
        _inputManager._PlayerCrouchAction.canceled += OnCrouchCanceled;
    }

    private void UnsubscribeFromInput() {
        if (_inputManager == null) return;

        _inputManager._PlayerMoveAction.performed -= OnMove;
        _inputManager._PlayerMoveAction.canceled -= OnMove;

        _inputManager._PlayerJumpAction.started -= OnJumpStarted;
        _inputManager._PlayerJumpAction.canceled -= OnJumpCanceled;

        _inputManager._PlayerCrouchAction.started -= OnCrouchStarted;
        _inputManager._PlayerCrouchAction.canceled -= OnCrouchCanceled;
    }

    #endregion

    #region Input Callbacks

    private void OnMove(InputAction.CallbackContext context) {
        _blackboard.MoveInput = context.ReadValue<Vector2>();
    }

    private void OnJumpStarted(InputAction.CallbackContext context) {
        // Set sustained jump (for variable jump height)
        _blackboard.IsJumpSustained = true;

        // Set jump buffer (gives player a small window to jump even if they pressed slightly early)
        _blackboard.JumpBufferTimer = _stats.JumpBufferTime;

        // Also set immediate jump press for instant state checks
        _blackboard.IsJumpPressed = true;

        // Start buffer countdown routine
        if (_jumpBufferRoutine != null) {
            StopCoroutine(_jumpBufferRoutine);
        }
        _jumpBufferRoutine = StartCoroutine(JumpBufferRoutine());
    }

    private void OnJumpCanceled(InputAction.CallbackContext context) {
        // Player released jump button
        _blackboard.IsJumpSustained = false;
    }

    private IEnumerator JumpBufferRoutine() {
        // Keep jump pressed flag true for buffer duration
        yield return new WaitForSeconds(_stats.JumpBufferTime);
        _blackboard.IsJumpPressed = false;
    }

    private void OnCrouchStarted(InputAction.CallbackContext context) {
        _blackboard.IsCrouchPressed = true;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context) {
        _blackboard.IsCrouchPressed = false;
    }

    #endregion

    /// <summary>
    /// Consume the jump buffer (called by states when jump is executed)
    /// </summary>
    public void ConsumeJumpBuffer() {
        _blackboard.JumpBufferTimer = 0;
        _blackboard.IsJumpPressed = false;
    }

    /// <summary>
    /// Check if jump input is active (either buffered or currently pressed)
    /// </summary>
    public bool HasJumpInput() {
        return _blackboard.JumpBufferTimer > 0 || _blackboard.IsJumpPressed;
    }
}