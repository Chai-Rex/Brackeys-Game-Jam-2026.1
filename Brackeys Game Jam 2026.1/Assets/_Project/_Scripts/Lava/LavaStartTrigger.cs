using UnityEngine;

public class LavaStartTrigger : MonoBehaviour {
    [SerializeField] private RisingLava _lava;
    [SerializeField] private HeightWinCondition _winCondition;
    [SerializeField] private LayerMask _triggerLayers;
    [SerializeField] private bool _triggerOnce = true;
    [SerializeField] private GameObject _iItem;

    private bool _hasTriggered;

    private void OnTriggerEnter2D(Collider2D other) {
        if (_hasTriggered && _triggerOnce)
            return;

        if (((1 << other.gameObject.layer) & _triggerLayers) == 0)
            return;

        _lava.StartLavaRise();
        _winCondition.StartWinCondition();

        _hasTriggered = true;

        _iItem.SetActive(false);
    }
}