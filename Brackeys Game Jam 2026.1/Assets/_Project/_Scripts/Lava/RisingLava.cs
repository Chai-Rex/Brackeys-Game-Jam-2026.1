using UnityEngine;
using System.Collections;

public class RisingLava : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private UIGamePlayHandler _uiGamePlayHandler;

    [Header("Base Movement")]
    [SerializeField] private float _baseRiseSpeed = 2f;

    [Header("Catch-Up System")]
    [SerializeField] private float _pressureRange = 6f;
    [SerializeField] private float _maxCatchupMultiplier = 6f;
    [SerializeField] private float _catchupRampSpeed = 0.75f;

    [Header("Pressure Phase")]
    [SerializeField] private float _pressureDuration = 3f;
    [SerializeField]
    private AnimationCurve _pressureCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _lavaRoutine;

    private float _catchupTimer;
    private float _currentCatchupMultiplier = 1f;
    private bool _pressureActive;

    /* ------------------------------------------------------------ */

    public void StartLavaRise() {
        if (_lavaRoutine != null)
            StopCoroutine(_lavaRoutine);

        _lavaRoutine = StartCoroutine(LavaRiseRoutine());
    }

    public void StopLavaRise() {
        if (_lavaRoutine != null)
            StopCoroutine(_lavaRoutine);

        _lavaRoutine = null;
        _pressureActive = false;
        _catchupTimer = 0f;
        _currentCatchupMultiplier = 1f;
    }

    /* ------------------------------------------------------------ */

    private IEnumerator LavaRiseRoutine() {
        while (true) {
            if (_player == null)
                yield break;

            float lavaY = transform.position.y;
            float playerY = _player.position.y;
            float distance = playerY - lavaY;

            bool outOfRange = distance > _pressureRange;

            // Progressive catch-up acceleration
            if (outOfRange) {
                _catchupTimer += Time.deltaTime;

                _currentCatchupMultiplier = Mathf.Lerp(
                    1f,
                    _maxCatchupMultiplier,
                    1f - Mathf.Exp(-_catchupRampSpeed * _catchupTimer)
                );
            } else {
                _catchupTimer = 0f;
                _currentCatchupMultiplier = 1f;
            }

            if (!_pressureActive) {
                if (distance <= _pressureRange) {
                    yield return StartCoroutine(PressurePhase());
                } else {
                    float speed = _baseRiseSpeed * _currentCatchupMultiplier;

                    transform.position += Vector3.up * speed * Time.deltaTime;
                }
            }

            yield return null;
        }
    }

    /* ------------------------------------------------------------ */

    private IEnumerator PressurePhase() {
        _pressureActive = true;

        float timer = _pressureDuration;
        float startY = transform.position.y;
        float targetY = _player.position.y;

        while (timer > 0f) {
            if (_player == null)
                yield break;

            timer -= Time.deltaTime;

            float lavaY = transform.position.y;
            float playerY = _player.position.y;
            float distance = playerY - lavaY;

            // Player escaped -> abort pressure phase
            if (distance > _pressureRange) {
                _pressureActive = false;
                yield break;
            }

            float t = Mathf.Clamp01(1f - (timer / _pressureDuration));
            float curvedT = _pressureCurve.Evaluate(t);

            float newY = Mathf.Lerp(startY, targetY, curvedT);

            transform.position = new Vector3(
                transform.position.x,
                newY,
                transform.position.z
            );

            yield return null;
        }

        CatchPlayerButKeepRising();
    }

    /* ------------------------------------------------------------ */

    private void CatchPlayerButKeepRising() {
        Debug.Log("Player caught by lava!");
        _pressureActive = false;

        _uiGamePlayHandler.PlayerDeath();
    }
}