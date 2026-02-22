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
        _iTimerText.text = GetFormattedTime();
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

    public string GetFormattedTime() {
        int totalSeconds = Mathf.FloorToInt(_elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }   

}
