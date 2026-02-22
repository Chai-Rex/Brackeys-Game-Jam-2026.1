using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour {

    [SerializeField] private Button _button;
    [SerializeField] private string _soundEventName = "UI_BigButton";


    private void Awake() {
        _button.onClick.AddListener(PlaySound);
    }

    private void PlaySound() {
        AkUnitySoundEngine.PostEvent(_soundEventName, gameObject);
    }
}
