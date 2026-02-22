using UnityEngine;

public class LavaStartTrigger : MonoBehaviour {
    [SerializeField] private RisingLava _lava;
    [SerializeField] private LayerMask _triggerLayers;
    [SerializeField] private bool _triggerOnce = true;

    private bool _hasTriggered;

    private void OnTriggerEnter2D(Collider2D other) {
        if (_hasTriggered && _triggerOnce)
            return;

        // Check if object's layer is inside the mask
        if (((1 << other.gameObject.layer) & _triggerLayers) == 0)
            return;

        _lava.StartLavaRise();
        _hasTriggered = true;
    }
}