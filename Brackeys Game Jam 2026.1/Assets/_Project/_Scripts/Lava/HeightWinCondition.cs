using UnityEngine;

public class HeightWinCondition : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private Transform _player;
    [SerializeField] private float _winHeight = 50f;

    private bool _winConditionActive;
    private bool _hasWon;

    private void Update() {
        if (!_winConditionActive || _hasWon || _player == null)
            return;

        if (_player.position.y >= _winHeight) {
            TriggerWin();
        }
    }

    public void ActivateWinCondition() {
        _winConditionActive = true;
    }

    private void TriggerWin() {
        _hasWon = true;

        Debug.Log("Player Wins!");

    }
}