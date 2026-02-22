using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class CreditsCanvas : MonoBehaviour {
    //[Header("Level Loading")]
    //[SerializeField] private LevelManager.Levels levelToLoad;

    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    [SerializeField] private float scrollSpeed = 50f; // pixels per second

    [SerializeField] private TMP_Text _iDeathsText;

    private float _maxScrollY;
    private bool _isScrolling;

    private CancellationTokenSource _cancellationTokenSource;

    private void Awake() {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void OnDestroy() {
        // Cancel any running async work
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        //if (InputManager.Instance)
        //    InputManager.Instance._DialogueContinueAction.started -= _DialogueContinueAction_started;
    }

    public async void StartCredits() {
        CancellationToken token = _cancellationTokenSource.Token;

        try {
            //InputManager.Instance.DisablePlayerActions();

            await Awaitable.WaitForSecondsAsync(1f, token);

            //InputManager.Instance.EnableDialogueActions();
            //InputManager.Instance._DialogueContinueAction.started += _DialogueContinueAction_started;

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

    private void _DialogueContinueAction_started(
        UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        //LevelManager.Instance.LoadScene(levelToLoad);
    }
}
