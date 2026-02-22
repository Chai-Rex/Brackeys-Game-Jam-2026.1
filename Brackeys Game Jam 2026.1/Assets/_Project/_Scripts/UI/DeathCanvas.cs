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
    [SerializeField] private float _iFillSpeed = 1;

    [Header("Death Reason")]
    [SerializeField] private TMP_Text _iDeathText;

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

    public void SetCauseOfDeathText(string i_causeOfDeath) {
        _iDeathText.gameObject.SetActive(true);
        _iDeathText.text = i_causeOfDeath;
    }

    public async Task CloseEyes() {
        _iDeathText.gameObject.SetActive(false);
        _iUpperImage.fillAmount = 0;
        _iLowerImage.fillAmount = 0;

        while (_iUpperImage.fillAmount < 1) {
            _iUpperImage.fillAmount += _iFillSpeed * Time.deltaTime;
            _iLowerImage.fillAmount += _iFillSpeed * Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

    }

    public async Task OpenEyes() {
        _iDeathText.gameObject.SetActive(false);
        _iUpperImage.fillAmount = 1;
        _iLowerImage.fillAmount = 1;

        while (_iUpperImage.fillAmount > 0) {
            _iUpperImage.fillAmount -= _iFillSpeed * Time.deltaTime;
            _iLowerImage.fillAmount -= _iFillSpeed * Time.deltaTime;

            await Awaitable.NextFrameAsync();
        }

    }


    public void SetStats() {

    }

    public void RestartLevel() {
        _gameCommandsManager.LoadLevel(_iLevelToRestart);
    }

}
