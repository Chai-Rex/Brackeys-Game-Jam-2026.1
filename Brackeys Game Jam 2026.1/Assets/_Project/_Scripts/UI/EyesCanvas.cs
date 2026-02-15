using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EyesCanvas : MonoBehaviour {

    [SerializeField] private TMP_Text _iDeathText;

    [SerializeField] private Image _iUpperImage;

    [SerializeField] private Image _iLowerImage;

    [SerializeField] private float _iFillSpeed = 1;

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

}
