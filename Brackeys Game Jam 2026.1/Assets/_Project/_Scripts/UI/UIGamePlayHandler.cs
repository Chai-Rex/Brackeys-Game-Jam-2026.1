using UnityEngine;


public class UIGamePlayHandler : MonoBehaviour {

    [Header("")]
    [SerializeField] private SceneContainerSO _sceneContainer;

    [Header("Canvas")]
    [SerializeField] private PauseCanvas _iPauseCanvas;
    [SerializeField] private HUDCanvas _iHUDCanvas;
    [SerializeField] private CreditsCanvas _iCreditsCanvas;
    [SerializeField] private VideoCanvas _iVideoCanvas;
    [SerializeField] private DeathCanvas _iDeathCanvas;
    [SerializeField] private TimeCanvas _iTimeCanvas;


    [Header("Video play")]
    [SerializeField] private bool _iPlayVideoOnStart = false;

    private InputManager _inputManager;
    private GameCommandsManager _gameCommandsManager;

    private bool _isVideoPlaying = false;   

    public void Awake() {
        _inputManager = _sceneContainer.GetManager<InputManager>();
        _gameCommandsManager = _sceneContainer.GetManager<GameCommandsManager>();

        if (_iPauseCanvas != null) { Debug.Log("PauseCanvas found and assigned."); }
        _iPauseCanvas.AssignResumeAction(Resume);

        if (_iPlayVideoOnStart) {
            PlayVideoSequence();
        } else {
            StartGame();
        }

    }

    public void Start() {

        _inputManager._PlayerPauseAction.started += _PlayerPauseAction_started;
        _inputManager._UIResumeAction.started += _UIResumeAction_started;
    }

    public void OnDestroy() {
        _inputManager._PlayerPauseAction.started -= _PlayerPauseAction_started;
        _inputManager._UIResumeAction.started -= _UIResumeAction_started;
    }

    private void _PlayerPauseAction_started(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        _iPauseCanvas.gameObject.SetActive(true);
        _inputManager.SetUIActionMap();

        _iTimeCanvas.PauseTimer();

        _gameCommandsManager.PauseGame();
    }

    private void _UIResumeAction_started(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        Resume();
    }

    private void Resume() {
        if (_isVideoPlaying) {
            StartGame();
        }

        _iPauseCanvas.gameObject.SetActive(false);
        _inputManager.SetPlayerActionMap();

        _iTimeCanvas.ResumeTimer();

        _gameCommandsManager.ResumeGame();
    }


    private void PlayVideoSequence() {

        _iVideoCanvas.gameObject.SetActive(true);
        _iHUDCanvas.gameObject.SetActive(false);
        _iPauseCanvas.gameObject.SetActive(false);
        _iCreditsCanvas.gameObject.SetActive(false);
        _iDeathCanvas.gameObject.SetActive(false);
        _iTimeCanvas.gameObject.SetActive(false);

        _isVideoPlaying = true;

        _inputManager.SetUIActionMap();
        _iVideoCanvas.PlayVideo(StartGame);
    }

    private void StartGame() {
        _isVideoPlaying = false;

        _iVideoCanvas.gameObject.SetActive(false);
        _iHUDCanvas.gameObject.SetActive(true);
        _iPauseCanvas.gameObject.SetActive(false);
        _iCreditsCanvas.gameObject.SetActive(false);
        _iDeathCanvas.gameObject.SetActive(false);
        _iTimeCanvas.gameObject.SetActive(true);

        _iTimeCanvas.StartTimer();

        _inputManager.SetPlayerActionMap();

    }
}
