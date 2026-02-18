using UnityEngine;

public class AkLoadBankOnAwake : MonoBehaviour
{
    [SerializeField] string in_psz = "SFX";
    void Awake()
    {
        AkUnitySoundEngine.LoadBank(in_psz, out _);
    }
}
