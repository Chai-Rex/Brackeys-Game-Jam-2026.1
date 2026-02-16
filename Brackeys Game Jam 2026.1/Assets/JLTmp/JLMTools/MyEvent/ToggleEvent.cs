using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class ToggleEvent
{
    [SerializeField] UnityEvent<bool> persistent = new();
    List<UnityAction<bool>> dynamic = new();
    bool isOn = false;

    public bool IsOn => isOn;
    
    public void Add(UnityAction<bool> call)
    {
        dynamic.Add(call);

        if (isOn)
            call?.Invoke(true);
    } 

    public bool Remove(UnityAction<bool> call)
    {
        bool hasRemoved = dynamic.Remove(call);

        if (isOn &&
            hasRemoved)
            call?.Invoke(false);
        
        return hasRemoved;
    } 

    public int RemoveAll(UnityAction<bool> call)
    {
        int nbRemoved = dynamic.RemoveAll((c) => c == call);

        if (isOn && call != null)
            for (int i = 0; i < nbRemoved; i++)
                call.Invoke(false);

        return nbRemoved;
    } 

    public void Clear()
    {
        IEnumerable<UnityAction<bool>> calls = dynamic.NotNull();

        dynamic.Clear();

        if (isOn)
            foreach (UnityAction<bool> call in calls)
                call.Invoke(false);
    } 
    
    public bool Contains(UnityAction<bool> call) => dynamic.Contains(call);

    void Invoke(bool boolean)
    {
        persistent.Invoke(boolean);

        foreach (UnityAction<bool> call in dynamic.ToArray())
            call?.Invoke(boolean); 
    }

    public void Set(bool isOn) 
    {
        if (this.isOn == isOn)
            return;

        this.isOn = isOn;
        Invoke(isOn);
    }
}