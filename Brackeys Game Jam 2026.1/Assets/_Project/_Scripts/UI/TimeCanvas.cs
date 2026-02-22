using DG.Tweening;
using TMPro;
using UnityEngine;

public class TimeCanvas : MonoBehaviour {
    [SerializeField] private TMP_Text _iTimerText;

    private float _elapsedTime;
    private bool _isRunning;

    private void Update() {
        if (!_isRunning)
            return;

        _elapsedTime += Time.unscaledDeltaTime;
        UpdateDisplay();
    }

    private void UpdateDisplay() {
        int totalSeconds = Mathf.FloorToInt(_elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        _iTimerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartTimer() {
        _elapsedTime = 0f;
        _isRunning = true;
        UpdateDisplay();
    }

    public void PauseTimer() {
        _isRunning = false;
    }

    public void ResumeTimer() {
        _isRunning = true;
    }

    public void ResetTimer() {
        _elapsedTime = 0f;
        _isRunning = false;
        UpdateDisplay();
    }

}
