using SoundSystem;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseCanvas : MonoBehaviour {

    [Header("References")]
    [SerializeField] private SceneContainerSO _iSceneContainer;

    [Header("Buttons")]
    [SerializeField] private Button _iResumeButton;
    [SerializeField] private Button _iMainMenuButton;
    [SerializeField] private Button _iQuitButton;

    [Header("Sliders")]
    [SerializeField] private Slider _iMasterVolumeSlider;
    [SerializeField] private Slider _iMusicVolumeSlider;
    [SerializeField] private Slider _iEffectsVolumeSlider;


    private SaveManager _saveManager;
    private GameCommandsManager _gameCommandsManager;

    private Action _onResume;

    private SaveUISettingsSO _settings;

    private void Start() {
        _saveManager = _iSceneContainer.GetManager<SaveManager>();
        _gameCommandsManager = _iSceneContainer.GetManager<GameCommandsManager>();

        _settings = _saveManager.SaveSettingsSO;

        _iMasterVolumeSlider.value = _settings.MasterVolume;
        _iMusicVolumeSlider.value = _settings.MusicVolume;
        _iEffectsVolumeSlider.value = _settings.EffectsVolume;

        // Slider
        _iMasterVolumeSlider.onValueChanged.AddListener((float value) => { 
            _settings.MasterVolume = value;
            AkUnitySoundEngine.SetState("Master_Volume", ((uint)(value * 100)).ToString());
        });
        _iMusicVolumeSlider.onValueChanged.AddListener((float value) => { 
            _settings.MusicVolume = value; 
            AkUnitySoundEngine.SetState("Music_Volume", ((uint)(value * 100)).ToString());
        });
        _iEffectsVolumeSlider.onValueChanged.AddListener((float value) => { 
            _settings.EffectsVolume = value;
            AkUnitySoundEngine.SetState("Music_Volume", ((uint)(value * 100)).ToString());
        });

        // Buttons
        _iResumeButton.onClick.AddListener(Resume);
        _iMainMenuButton.onClick.AddListener(ReturnToMenu);
        _iQuitButton.onClick.AddListener(Quit);
    }

    private void OnDestroy() {

        _iMasterVolumeSlider.onValueChanged.RemoveAllListeners();
        _iMusicVolumeSlider.onValueChanged.RemoveAllListeners();
        _iEffectsVolumeSlider.onValueChanged.RemoveAllListeners();

        _iResumeButton.onClick.RemoveAllListeners();
        _iMainMenuButton.onClick.RemoveAllListeners();
        _iQuitButton.onClick.RemoveAllListeners();
    }


    public void AssignResumeAction(Action i_resume) {
        _onResume = i_resume;
    }

    public void Resume() {
        if (_onResume == null)
            Debug.LogError("PauseCanvas: Resume action is null!");
        _onResume();
        _saveManager.SaveSettings();
    }

    public void ReturnToMenu() {
        _gameCommandsManager.QuitToMainMenu();
        gameObject.SetActive(false);

    }

    public void Quit() {
        Application.Quit();
    }

    public void FullScreen() {
        Screen.fullScreen = !Screen.fullScreen;
    }

}
