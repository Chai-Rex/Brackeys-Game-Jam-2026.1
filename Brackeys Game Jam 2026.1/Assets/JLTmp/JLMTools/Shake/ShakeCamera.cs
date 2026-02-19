using UnityEngine;


public class ShakeCamera : Shake
{
    static ShakeCamera instance = null;
    static public ShakeCamera Instance => instance;


    protected override void Awake()
    {
        base.Awake();
        CacheInstance();
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void CacheInstance()
    {
        if (instance)
        {
            Debug.LogError("ShakeCamera : instance != null");
            return;
        }

        instance = this;
    }

    public void Impact(float amplitude, Vector3 position, float radius)
    {
        if (!ShakeCameraListener.Instance)
            return;

        float sqrDist = (position - ShakeCameraListener.Instance.transform.position).sqrMagnitude; 

        if (sqrDist >= radius * radius)
            return;
        
        float dist = Mathf.Sqrt(sqrDist);
        float coef = 1 - dist / radius;
        Impact(amplitude * coef);
    }

    public void Impact(float amplitude, Vector2 position, float radius)
    {
        if (!ShakeCameraListener.Instance)
            return;

        float sqrDist = (position - (Vector2)ShakeCameraListener.Instance.transform.position).sqrMagnitude; 

        if (sqrDist >= radius * radius)
            return;
        
        float dist = Mathf.Sqrt(sqrDist);
        float coef = 1 - dist / radius;
        Impact(amplitude * coef);
    }
}
