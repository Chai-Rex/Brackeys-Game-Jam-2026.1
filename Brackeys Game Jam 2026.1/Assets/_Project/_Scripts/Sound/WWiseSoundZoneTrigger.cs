using UnityEngine;

public class WWiseSoundZoneTrigger : MonoBehaviour {

    [SerializeField] private LayerMask _triggerLayers;
    [SerializeField] private string _soundEventName = "Cyberpunk_Play";

    private void OnTriggerEnter2D(Collider2D other) {

        if (((1 << other.gameObject.layer) & _triggerLayers) == 0)
            return;

        AkUnitySoundEngine.PostEvent(_soundEventName, Camera.main.gameObject);

        Debug.Log("Playing: " + _soundEventName);

    }
}
