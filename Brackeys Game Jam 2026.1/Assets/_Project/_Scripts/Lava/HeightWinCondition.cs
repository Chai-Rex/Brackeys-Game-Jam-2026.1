using System.Collections;
using UnityEngine;

public class HeightWinCondition : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private Transform _player;
    [SerializeField] private float _winHeight = 50f;

    [SerializeField] private UIGamePlayHandler _uiGamePlayHandler;

    public void StartWinCondition() {
        StartCoroutine(WinConditionRoutine());
    }
    
    private IEnumerator WinConditionRoutine() {
        while (_player.position.y >= _winHeight) {
            yield return null;
        }
        TriggerWin();
    }

    private void TriggerWin() {

        Debug.Log("Player Wins!");
        _uiGamePlayHandler.Victory();

    }
}