using UnityEngine;


public class Shake : Singleton<Shake>
{    
    [SerializeField] Vector3 axis = new Vector3(1, 1, 0);
    [SerializeField] float baseAmplitude = 0;
    [SerializeField] float baseNoise = 5;
    [SerializeField] float addAmplitude = 0;
    [SerializeField] float addNoise = 0;
    [SerializeField] float speed = 5;
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
    
    public float AddAmplitude
    {
        get => addAmplitude;
        set => addAmplitude = value;
    }

    public float AddNoise
    {
        get => addNoise;
        set => addNoise = value;
    }

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public float Amplitude => baseAmplitude + addAmplitude;

    public float Noise => baseNoise + addNoise;

    protected override void Awake()
    {
        base.Awake();
        noisePosition = RandomExtension.RandomVector3(0, 1000);
    }

    void Update()
    {
        addAmplitude /= 1 + Time.deltaTime * speed;
        addNoise /= 1 + Time.deltaTime * speed;

        noisePosition += Time.deltaTime * Noise * Vector3.one;

        target.localPosition = new Vector3(
            (Mathf.PerlinNoise1D(noisePosition.x) - 0.5f) * axis.x * Amplitude,
            (Mathf.PerlinNoise1D(noisePosition.y) - 0.5f) * axis.y * Amplitude,
            (Mathf.PerlinNoise1D(noisePosition.z) - 0.5f) * axis.z * Amplitude);
    }
}
