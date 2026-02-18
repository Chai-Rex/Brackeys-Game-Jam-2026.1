using UnityEngine;


public class Shake : MonoBehaviour
{    
    [SerializeField] Vector3 axis = new Vector3(1, 1, 0);
    [SerializeField] float baseAmplitude = 0;
    [SerializeField] float baseNoise = 5;
    [SerializeField] float impactAmplitude = 0;
    [SerializeField] float impactNoise = 0;
    [SerializeField] float impactSpeed = 5;
    [SerializeField] Transform target;
    Vector3 noisePosition;

    public Vector2 Axis
    {
        get => axis;
        set => axis = value;
    }

    public float BaseAmplitude
    {
        get => baseAmplitude;
        set => baseAmplitude = value;
    }

    public float BaseNoise
    {
        get => baseNoise;
        set => baseNoise = value;
    }
    
    public float ImpactAmplitude
    {
        get => impactAmplitude;
        set => impactAmplitude = value;
    }

    public float ImpactNoise
    {
        get => impactNoise;
        set => impactNoise = value;
    }

    public float ImpactSpeed
    {
        get => impactSpeed;
        set => impactSpeed = value;
    }

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public void Impact(float amplitude, float noise = 0)
    {
        impactAmplitude += amplitude;
        impactNoise += noise;
    }

    public float Amplitude => baseAmplitude + ImpactAmplitude;
    public float Noise => baseNoise + impactNoise;

    protected virtual void Awake()
    {
        noisePosition = RandomExtension.RandomVector3(0, 1000);
    }

    void Update()
    {
        impactAmplitude *= 1 - impactSpeed * Time.deltaTime;
        impactNoise *= 1 - impactSpeed * Time.deltaTime;

        if (target)
        {
            noisePosition += Time.deltaTime * Noise * Vector3.one;

            target.localPosition = new Vector3(
                (Mathf.PerlinNoise1D(noisePosition.x) - 0.5f) * axis.x * Amplitude,
                (Mathf.PerlinNoise1D(noisePosition.y) - 0.5f) * axis.y * Amplitude,
                (Mathf.PerlinNoise1D(noisePosition.z) - 0.5f) * axis.z * Amplitude);
        }
    }
}
