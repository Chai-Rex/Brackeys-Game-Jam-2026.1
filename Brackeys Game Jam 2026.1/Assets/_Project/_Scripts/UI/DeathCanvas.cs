using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathCanvas : MonoBehaviour {

    [Header("References")]
    [SerializeField] private SceneContainerSO _sceneContainer;

    [Header("Animation")]
    [SerializeField] private Image _iUpperImage;
    [SerializeField] private Image _iLowerImage;
    [SerializeField] private Image _iBackgroundImage;
    [SerializeField] private float _iFillSpeed = 1;

    [Header("Stats")]
    [SerializeField] private TMP_Text _iTime;
    [SerializeField] private TMP_Text _iBlocksBroken;
    [SerializeField] private TMP_Text _iDistanceTraveled;

    [Header("Restart")]
    [SerializeField] private string _iLevelToRestart = "GameplayScene";
    [SerializeField] private Button _iRestartButton;


    private PlayerManager _playerManager;
    private GameCommandsManager _gameCommandsManager;

    private void Start() {
        _playerManager = _sceneContainer.GetManager<PlayerManager>();
        _gameCommandsManager = _sceneContainer.GetManager<GameCommandsManager>();
        _iRestartButton.onClick.AddListener(RestartLevel);
    }

    public async Task CloseEyes() {
        DisableText();
        _iUpperImage.fillAmount = 0;
        _iLowerImage.fillAmount = 0;

        while (_iUpperImage.fillAmount < 1) {
            _iUpperImage.fillAmount += _iFillSpeed * Time.deltaTime;
            _iLowerImage.fillAmount += _iFillSpeed * Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

    }

    public async Task OpenEyes() {
        DisableText();
        _iUpperImage.fillAmount = 1;
        _iLowerImage.fillAmount = 1;

        while (_iUpperImage.fillAmount > 0) {
            _iUpperImage.fillAmount -= _iFillSpeed * Time.deltaTime;
            _iLowerImage.fillAmount -= _iFillSpeed * Time.deltaTime;

            await Awaitable.NextFrameAsync();
        }

    }

    private void DisableText() {
        _iTime.gameObject.SetActive(false);
        _iBlocksBroken.gameObject.SetActive(false);
        _iDistanceTraveled.gameObject.SetActive(false);

        _iRestartButton.gameObject.SetActive(false);

        _iBackgroundImage.gameObject.SetActive(false);
    }


    public void SetStats(string time, int distance, int blocks = 0) {
        _iTime.gameObject.SetActive(true);
        _iBlocksBroken.gameObject.SetActive(true);
        _iDistanceTraveled.gameObject.SetActive(true);

        _iRestartButton.gameObject.SetActive(true);

        _iBackgroundImage.gameObject.SetActive(true);

        _iTime.text = $"Time: {time}";
        _iBlocksBroken.text = $"Blocks Broken: {blocks}";
        _iDistanceTraveled.text = $"Distance Traveled: {distance}m";
    }

    public void RestartLevel() {
        _gameCommandsManager.LoadLevel(_iLevelToRestart);
    }

}
