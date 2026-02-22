using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDCanvas : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Button _iSkillsButton;
    [SerializeField] private GameObject _iSkillCanvas;

    private void Start() {
        _iSkillsButton.onClick.AddListener(OpenSkills);
    }
    private void OnDestroy() {
        _iSkillsButton.onClick.RemoveAllListeners();
    }   

    private void OpenSkills() {
        _iSkillCanvas.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }


}
