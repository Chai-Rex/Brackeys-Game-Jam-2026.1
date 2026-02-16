using UnityEngine;


public static class UnityExtension
{
    public static bool TryGetComponentInParent<T>(this Component component, out T componentOut, bool includeInactive = true)
    {
        componentOut = component.GetComponentInParent<T>(includeInactive);
        return componentOut != null;
    }

    public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T componentOut, bool includeInactive = true)
    {
        componentOut = gameObject.GetComponentInParent<T>(includeInactive);
        return componentOut != null;
    }

    public static bool TryGetComponentInChildren<T>(this Component component, out T componentOut, bool includeInactive = true)
    {
        componentOut = component.GetComponentInChildren<T>(includeInactive);
        return componentOut != null;
    }

    public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T componentOut, bool includeInactive = true)
    {
        componentOut = gameObject.GetComponentInChildren<T>(includeInactive);
        return componentOut != null;
    }

    public static bool TryGetComponentInParentOrInChildren<T>(this Component component, out T componentOut, bool includeInactive = true)
        => component.TryGetComponentInParent(out componentOut, includeInactive) || 
           component.TryGetComponentInChildren(out componentOut, includeInactive);

    public static bool TryGetComponentInParentOrInChildren<T>(this GameObject gameObject, out T componentOut, bool includeInactive = true)
        => gameObject.TryGetComponentInParent(out componentOut, includeInactive) || 
           gameObject.TryGetComponentInChildren(out componentOut, includeInactive);

    public static Transform[] GetChildren(this Transform parent)
    {
        if (!parent)
            return new Transform[0];

        Transform[] children = new Transform[parent.childCount];

        for (int i = 0; i < parent.childCount; i++)
            children[i] = parent.GetChild(i);

        return children;
    }

    public static Transform Find(this Transform parent, string childName, bool deep = false)
    {
        foreach (Transform crtChild in parent)
        {
            if (crtChild.name == childName)
                return crtChild;

            if (deep && crtChild.TryFind(childName, out Transform child, true))
                return child;
        }
        return null;
    }

    public static bool TryFind(this Transform parent, string childName, out Transform child, bool deep = false)
    {
        child = parent.Find(childName, deep);
        return child != null;
    }

    public static T Instantiate<T>(this Transform transform, T prefab) where T : Object
    {
        if (!prefab)
        {
            Debug.LogError("UnityExtension.Instantiate : Missing prefab...");
            return null;
        }

        if (!transform)
            return Object.Instantiate(prefab);

        return Object.Instantiate(prefab, transform.position, transform.rotation, transform);
    }

    public static void SetX(this Transform transform, float x)
    {
        Vector3 position = transform.position;
        position.x = x;
        transform.position = position;
    }
    
    public static void SetY(this Transform transform, float y)
    {
        Vector3 position = transform.position;
        position.y = y;
        transform.position = position;
    }

    public static void SetZ(this Transform transform, float z)
    {
        Vector3 position = transform.position;
        position.z = z;
        transform.position = position;
    }
}


