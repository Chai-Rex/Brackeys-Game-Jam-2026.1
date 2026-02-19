using EditorAttributes;
using UnityEngine;


public class Shake : MonoBehaviour
{    
    [SerializeField] Transform target;

    [Space(10)]
    [SerializeField] float baseAmplitude = 0;
    [SerializeField] float impactAmplitude = 0;
    [SerializeField] float impactSpeed = 5;

    [Space(10)]
    [SerializeField] float baseNoise = 0;
    [SerializeField] float noiseWithAmplitude = 100;
    Vector3 positionNoisePos;
    Vector3 rotationNoisePos;

    [Space(10)]
    [SerializeField] ShakeType shakeType = ShakeType.Position;
    bool isShakingPosition = false;
    bool isShakingRotation = false;

    [SerializeField, ShowField(nameof(isShakingPosition))] Vector3 shakePositionAxis = new Vector3(1, 1, 0);
    [SerializeField, ShowField(nameof(isShakingPosition))] float shakePositionWithAmplitude = 1;

    [SerializeField, ShowField(nameof(isShakingRotation))] Vector3 shakeRotationAxis = new Vector3(1, 1, 0);
    [SerializeField, ShowField(nameof(isShakingRotation))] float shakeRotationWithAmplitude = 10;


    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public float BaseAmplitude
    {
        get => baseAmplitude;
        set => baseAmplitude = value;  
    }

    public float ImpactAmplitude
    {
        get => impactAmplitude;
        set => impactAmplitude = value;  
    }

    public float ImpactSpeed
    {
        get => impactSpeed;
        set => impactSpeed = value;  
    }

    public float Amplitude => baseAmplitude + impactAmplitude;

    public float BaseNoise
    {
        get => baseNoise;
        set => baseNoise = value;  
    }

    public float NoiseWithAmplitude
    {
        get => noiseWithAmplitude;
        set => noiseWithAmplitude = value;  
    }

    public float Noise => baseNoise + noiseWithAmplitude * Amplitude;

    public ShakeType ShakeType
    {
        get => shakeType;

        set {
            shakeType = value;
            isShakingPosition = shakeType == ShakeType.Position || shakeType == ShakeType.Both;
            isShakingRotation = shakeType == ShakeType.Rotation || shakeType == ShakeType.Both;            
        }
    }

    public Vector3 ShakePositionAxis
    {
        get => shakePositionAxis;
        set => shakePositionAxis = value;
    }

    public float ShakePositionWithAmplitude
    {
        get => shakePositionWithAmplitude;
        set => shakePositionWithAmplitude = value;
    }

    public Vector3 ShakeRotationAxis
    {
        get => shakeRotationAxis;
        set => shakeRotationAxis = value;
    }

    public float ShakeRotationWithAmplitude
    {
        get => shakeRotationWithAmplitude;
        set => shakeRotationWithAmplitude = value;
    }
    
    protected virtual void Reset()
    {
        if (transform.childCount == 1)
            target = transform.GetChild(0);
    }

    void OnValidate()
    {
        isShakingPosition = shakeType == ShakeType.Position || shakeType == ShakeType.Both;
        isShakingRotation = shakeType == ShakeType.Rotation || shakeType == ShakeType.Both;     
    }

    protected virtual void Awake()
    {
        positionNoisePos = RandomExtension.RandomVector3(0, 1000);
        rotationNoisePos = RandomExtension.RandomVector3(0, 1000);

        isShakingPosition = shakeType == ShakeType.Position || shakeType == ShakeType.Both;
        isShakingRotation = shakeType == ShakeType.Rotation || shakeType == ShakeType.Both;     
    }

    void Update()
    {
        
        if (impactAmplitude < 0.00001) impactAmplitude = 0;
        else impactAmplitude *= 1 - impactSpeed * Time.deltaTime;

        if (!target)
            return;

        if (isShakingPosition)
        {
            positionNoisePos += Noise * Time.deltaTime * Vector3.one;
            Vector3 shakePosition = shakePositionWithAmplitude * Amplitude * shakePositionAxis;
            target.localPosition = Vector3.Scale(RandomExtension.PerlinVector3(positionNoisePos), shakePosition);
        }

        if (isShakingRotation)
        {
            rotationNoisePos += Noise * Time.deltaTime * Vector3.one;
            Vector3 shakeRotation = shakeRotationWithAmplitude * Amplitude * shakeRotationAxis;
            target.eulerAngles = Vector3.Scale(RandomExtension.PerlinVector3(rotationNoisePos), shakeRotation);
        }
    }

    public void Impact(float amplitude) => impactAmplitude += amplitude;
}

public enum ShakeType
{
    None,
    Position,
    Rotation,
    Both
}
