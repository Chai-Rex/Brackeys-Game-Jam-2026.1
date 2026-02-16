using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;



[Serializable]
public class ToggleCounterEvent
{
    [SerializeField] UnityEvent<bool> persistent = new();
    List<UnityAction<bool>> dynamic = new();
    int value = 0;

    public int Value => value;
    public bool IsOn => value > 0;

    public void Add(UnityAction<bool> call)
    {
        dynamic.Add(call);

        if (IsOn)
            call?.Invoke(true);
    } 

    public bool Remove(UnityAction<bool> call)
    {
        bool hasRemoved = dynamic.Remove(call);

        if (IsOn &&
            hasRemoved)
            call?.Invoke(false);
        
        return hasRemoved;
    } 

    public int RemoveAll(UnityAction<bool> call)
    {
        int nbRemoved = dynamic.RemoveAll((c) => c == call);

        if (IsOn && 
            call != null)
            for (int i = 0; i < nbRemoved; i++)
                call.Invoke(false);

        return nbRemoved;
    } 

    public void Clear()
    {
        IEnumerable<UnityAction<bool>> calls = dynamic.NotNull();

        dynamic.Clear();

        if (IsOn)
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
    
    public void Increment()
    {
        value++;

        if (value == 1)
            Invoke(true);
    }

    public void Decrement()
    {
        value--;

        if (value == 0)
            Invoke(false);
    }
    public void InOrDecrement(bool increment)
    {
        if (increment) Increment();
        else           Decrement();
    }
}