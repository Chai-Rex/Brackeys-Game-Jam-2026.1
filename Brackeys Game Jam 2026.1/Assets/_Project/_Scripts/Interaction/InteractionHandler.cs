using UnityEngine;
using UnityEngine.Events;

public class InteractionHandler : MonoBehaviour {

    // TO DO: Add interaction events. consider static?


    [Header("Raycast Settings")]
    [SerializeField] private Transform _iCameraTransform;
    [SerializeField] private float _iInteractionRange = 3f;
    [SerializeField] private LayerMask _iInteractableLayer;
    [SerializeField] private LayerMask _iInHandLayer;

    [Header("Debug")]
    [SerializeField] private bool _iShowDebugRay = true;

    private IInteractable _currentInteractable;
    private GameObject _currentInteractableObject;

    private GameObject _currentInHandObject;

    public IInteractable CurrentInteractable => _currentInteractable;
    public bool HasInteractable => _currentInteractable != null;

    private void Awake() {
        if (_iCameraTransform == null) {
            _iCameraTransform = Camera.main.transform;
        }
    }

    private void Update() {
        CheckForInteractable();
    }


    // first person
    private void CheckForInteractable() {
        Ray ray = new Ray(_iCameraTransform.position, _iCameraTransform.forward);

        if (_iShowDebugRay) {
            Debug.DrawRay(ray.origin, ray.direction * _iInteractionRange,
                _currentInteractable != null ? Color.green : Color.red);
        }

        if (Physics.Raycast(ray, out RaycastHit hitInteractable, _iInteractionRange, _iInteractableLayer)) {
            IInteractable interactable = hitInteractable.collider.GetComponent<IInteractable>();

            if (interactable != null) {
                // New interactable found
                if (_currentInteractable != interactable) {
                    // Exit previous interactable
                    if (_currentInteractable != null) {
                        _currentInteractable.OnLookExit(gameObject);
                    }

                    // Enter new interactable
                    _currentInteractable = interactable;
                    _currentInteractableObject = hitInteractable.collider.gameObject;
                    _currentInteractable.OnLookEnter(gameObject);
                }
                return;
            }
        }

        if (Physics.Raycast(ray, out RaycastHit hitHand, _iInteractionRange, _iInHandLayer)) {
            if (_currentInHandObject != hitHand.collider.gameObject) {
                _currentInHandObject = hitHand.collider.gameObject;
            }
            return;
        }

        // No interactable found or lost line of sight
        if (_currentInteractable != null) {
            _currentInteractable.OnLookExit(gameObject);
            _currentInteractable = null;
            _currentInteractableObject = null;
        }

        // No InHand item found or lost line of sight
        if (_currentInHandObject != null) {
            _currentInHandObject = null;
        }
    }


    // Public methods
    public void SetInteractionRange(float i_range) {
        _iInteractionRange = i_range;
    }

    public float GetInteractionRange() {
        return _iInteractionRange;
    }

}
