using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class MyEvent
{
    [SerializeField] UnityEvent persistent = new();
    List<UnityAction> dynamic = new();

    public void Add(UnityAction call) => dynamic.Add(call);
    public bool Remove(UnityAction call) => dynamic.Remove(call);
    public int RemoveAll(UnityAction call) => dynamic.RemoveAll((c) => c == call);
    public void Clear() => dynamic.Clear();
    public bool Contains(UnityAction call) => dynamic.Contains(call);
    public UnityAction AddOneCall(UnityAction call)
    {
        UnityAction callThanRemove = null;

        callThanRemove = () =>
        {
            call?.Invoke();
            dynamic.Remove(callThanRemove);
        };

        dynamic.Add(callThanRemove);
        return callThanRemove;
    }
    public void Invoke() 
    {
        persistent.Invoke();

        foreach (UnityAction call in dynamic.ToArray())
            call?.Invoke(); 
    }
}

[Serializable]
public class MyEvent<T>
{
    [SerializeField] UnityEvent<T> persistent = new();
    List<UnityAction<T>> dynamic = new();


    public void Add(UnityAction<T> call) => dynamic.Add(call);
    public bool Remove(UnityAction<T> call) => dynamic.Remove(call);
    public int RemoveAll(UnityAction<T> call) => dynamic.RemoveAll((c) => c == call);
    public void Clear() => dynamic.Clear();
    public bool Contains(UnityAction<T> call) => dynamic.Contains(call);
    public UnityAction<T> AddOneCall(UnityAction<T> call)
    {
        UnityAction<T> callThanRemove = null;

        callThanRemove = (param) =>
        {
            call?.Invoke(param);
            dynamic.Remove(callThanRemove);
        };

        dynamic.Add(callThanRemove);
        return callThanRemove;
    }
    public void Invoke(T param)
    {
        persistent.Invoke(param);

        foreach (UnityAction<T> call in dynamic.ToArray())
            call?.Invoke(param);
    }
}


[Serializable]
public class MyEvent<T1, T2>
{
    [SerializeField] UnityEvent<T1, T2> persistent = new();
    List<UnityAction<T1, T2>> dynamic = new();


    public void Add(UnityAction<T1, T2> call) => dynamic.Add(call);
    public bool Remove(UnityAction<T1, T2> call) => dynamic.Remove(call);
    public int RemoveAll(UnityAction<T1, T2> call) => dynamic.RemoveAll((c) => c == call);
    public void Clear() => dynamic.Clear();
    public bool Contains(UnityAction<T1, T2> call) => dynamic.Contains(call);
    public UnityAction<T1, T2> AddOneCall(UnityAction<T1, T2> call)
    {
        UnityAction<T1, T2> callThanRemove = null;

        callThanRemove = (param1, param2) =>
        {
            call?.Invoke(param1, param2);
            dynamic.Remove(callThanRemove);
        };

        dynamic.Add(callThanRemove);
        return callThanRemove;
    }
    public void Invoke(T1 param1, T2 param2)
    {
        persistent.Invoke(param1, param2);

        foreach (UnityAction<T1, T2> call in dynamic.ToArray())
            call?.Invoke(param1, param2);
    }
}