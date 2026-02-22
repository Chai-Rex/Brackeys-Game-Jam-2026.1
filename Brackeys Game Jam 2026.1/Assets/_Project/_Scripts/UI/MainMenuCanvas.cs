using SoundSystem;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour {

    [Header("References")]
    [SerializeField] private SceneContainerSO _iSceneContainer;
    [SerializeField] private SaveManager _iSaveManager;
    [SerializeField] private GameCommandsManager _iGameCommandsManager;

    [Header("Buttons")]
    [SerializeField] private Button _iStartButton;
    [SerializeField] private Button _iQuitButton;

    [Header("Sliders")]
    [SerializeField] private Slider _iMasterVolumeSlider;

    [Header("Music")]
    [SerializeField] private string _iMusicEvent = "Menu_Play";

    private SaveUISettingsSO _settings;


    private void Start() {

        AkUnitySoundEngine.PostEvent(_iMusicEvent, Camera.main.gameObject);

        _iGameCommandsManager = _iSceneContainer.GetManager<GameCommandsManager>();
        //_iSaveManager = _iSceneContainer.GetManager<SaveManager>();

        _settings = _iSaveManager.SaveSettingsSO;

        _iMasterVolumeSlider.value = _settings.MasterVolume;

        // Slider
        _iMasterVolumeSlider.onValueChanged.AddListener((float value) => { 
            _settings.MasterVolume = value; 
            AkUnitySoundEngine.SetState("Master_Volume", ((int)(value * 100)).ToString());
        });

        // Buttons
        _iStartButton.onClick.AddListener(StartGame);
        _iQuitButton.onClick.AddListener(Quit);
    }

    private void OnDestroy() {

        _iMasterVolumeSlider.onValueChanged.RemoveAllListeners();

        _iStartButton.onClick.RemoveAllListeners();
        _iQuitButton.onClick.RemoveAllListeners();
    }

    private void OnDisable() {
        _iSaveManager.SaveSettings();
    }

    public void StartGame() {
        _iGameCommandsManager.BeginGame();
    }

    public void Quit() {
        Application.Quit();
    }

    public void FullSCreen() {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
