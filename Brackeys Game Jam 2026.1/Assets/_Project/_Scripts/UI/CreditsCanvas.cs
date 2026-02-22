using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class CreditsCanvas : MonoBehaviour {
    //[Header("Level Loading")]
    //[SerializeField] private LevelManager.Levels levelToLoad;

    [SerializeField] private SceneContainerSO _mainMenuSceneContainer;

    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    [SerializeField] private float scrollSpeed = 50f; // pixels per second

    [SerializeField] private TMP_Text _iDeathsText;
    [SerializeField] private SceneContainerSO _sceneContainer;

    private float _maxScrollY;
    private bool _isScrolling;

    private CancellationTokenSource _cancellationTokenSource;

    private InputManager _inputManager;
    private GameCommandsManager _gameCommandsManager;

    private void Awake() {
        _cancellationTokenSource = new CancellationTokenSource();
    }
    private void Start() {
        _inputManager = _sceneContainer.GetManager<InputManager>();
        _gameCommandsManager = _sceneContainer.GetManager<GameCommandsManager>();
    }
    private void OnDestroy() {
        // Cancel any running async work
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        _inputManager._PlayerJumpAction.started -= _PlayerJumpAction_started;
    }

    public async void StartCredits() {
        CancellationToken token = _cancellationTokenSource.Token;

        try {
            //InputManager.Instance.DisablePlayerActions();

            await Awaitable.WaitForSecondsAsync(1f, token);

            //InputManager.Instance.EnableDialogueActions();
            _inputManager._PlayerJumpAction.started += _PlayerJumpAction_started;

            // content height minus viewport height
            _maxScrollY = Mathf.Max(
                0f,
                content.rect.height - viewport.rect.height
            );

            _isScrolling = true;

            while (_isScrolling && !token.IsCancellationRequested) {
                Vector2 postion = content.anchoredPosition;
                postion.y += scrollSpeed * Time.deltaTime;

                if (postion.y >= _maxScrollY) {
                    postion.y = _maxScrollY;
                    _isScrolling = false;
                }

                content.anchoredPosition = postion;

                await Awaitable.NextFrameAsync(token);
            }
        } catch (OperationCanceledException) {
            // Expected on destroy — swallow safely
        }
    }

    private void _PlayerJumpAction_started(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        _gameCommandsManager.LoadLevel(_mainMenuSceneContainer);
    }
}
