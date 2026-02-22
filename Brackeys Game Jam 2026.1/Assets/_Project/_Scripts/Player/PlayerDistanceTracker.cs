using UnityEngine;

public class PlayerDistanceTracker : MonoBehaviour {
    public float TotalDistance { get; private set; }

    private Vector3 _lastPosition;
    private bool _initialized;

    private void Update() {
        if (!_initialized) {
            _lastPosition = transform.position;
            _initialized = true;
            return;
        }

        float distanceThisFrame = Vector3.Distance(transform.position, _lastPosition);
        TotalDistance += distanceThisFrame;

        _lastPosition = transform.position;
    }

    public void ResetDistance() {
        TotalDistance = 0f;
        _lastPosition = transform.position;
        _initialized = true;
    }
}