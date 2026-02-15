using TMPro;
using UnityEngine;

public class HUDCanvas : MonoBehaviour {

    [Header("References")]
    [SerializeField] private InteractionHandler _iInteractionHandler;
    [SerializeField] private GameObject _iInteractionPrompt;
    [SerializeField] private TextMeshProUGUI _iVerbText;

    //[Header("Settings")]
    //[SerializeField] private string _iKeyPrompt = "[E] ";

    private void Start() {
        if (_iInteractionHandler == null) {
            _iInteractionHandler = FindFirstObjectByType<InteractionHandler>();
        }

        // Hide prompt initially
        HideInteractionPrompt();
    }

    public void ShowInteractionPrompt(string verb) {
        _iInteractionPrompt.SetActive(true);

        if (_iVerbText != null) {
            _iVerbText.text = verb;
        }
    }
    public void ShowInteractionPrompt() {
        _iInteractionPrompt.SetActive(true);
    }
    public void HideInteractionPrompt() {
        _iInteractionPrompt.SetActive(false);
    }
}
