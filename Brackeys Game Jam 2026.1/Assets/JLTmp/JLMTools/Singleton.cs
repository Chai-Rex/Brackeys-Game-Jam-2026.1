using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    static T instance = null;

    public static T Instance => instance;

    protected virtual void Awake()
    {
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
            Debug.LogError("Singleton : instance != null");
            return;
        }

        if (this is not T)
        {
            Debug.LogError("Singleton : this is not T");
            return;
        }

        instance = (T)this;
    }
}
