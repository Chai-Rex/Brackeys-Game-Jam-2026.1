using DG.Tweening;
using TMPro;
using UnityEngine;

public class TimeCanvas : MonoBehaviour {
    [SerializeField] private TMP_Text _iTimeText;
    [SerializeField] private TMP_Text _iPauseText;

    [Header("Time Scaling")]
    [Tooltip("How many real-life seconds equal ONE in-game minute")]
    [SerializeField] private float secondsPerGameMinute = 1f;

    [Header("Start Time")]
    [Range(0, 23)]
    [SerializeField] private int startHour = 8;

    [Range(0, 59)]
    [SerializeField] private int startMinute = 0;

    [Header("Display")]
    [SerializeField] private bool useAmPmFormat = false;

    private void Start() {
        FadePausePrompt();
    }

    private void OnDestroy() {
        _iPauseText.DOKill();
    }

    /// <summary>
    /// Sets the displayed in-game time based on real seconds elapsed.
    /// </summary>
    public void SetTime(float realSecondsElapsed) {
        if (secondsPerGameMinute <= 0f)
            return;

        // Convert real seconds -> total in-game minutes
        float totalGameMinutes =
            realSecondsElapsed / secondsPerGameMinute;

        // Apply starting offset
        totalGameMinutes += startHour * 60f + startMinute;

        int hours = Mathf.FloorToInt(totalGameMinutes / 60f) % 24;
        int minutes = Mathf.FloorToInt(totalGameMinutes % 60f);

        if (useAmPmFormat) {
            int displayHour = hours % 12;
            if (displayHour == 0)
                displayHour = 12;

            string suffix = hours < 12 ? "AM" : "PM";
            _iTimeText.text = $"{displayHour:00}:{minutes:00} {suffix}";
        } else {
            _iTimeText.text = $"{hours:00}:{minutes:00}";
        }
    }

    private async void FadePausePrompt() {

        await Awaitable.WaitForSecondsAsync(5);
        _iPauseText.DOFade(0f, 3f);
    }
}
