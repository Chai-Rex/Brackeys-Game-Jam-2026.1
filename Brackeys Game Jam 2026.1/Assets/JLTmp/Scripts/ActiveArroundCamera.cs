using System.Collections.Generic;
using UnityEngine;

public class ActiveArroundCamera : MonoBehaviour
{
    static public HashSet<ActiveArroundCamera> set = new();

    void Awake()
    {
        if (enabled)
            set.Add(this);
    }

    void OnDestroy()
    {
        set.Remove(this);
    }
}
