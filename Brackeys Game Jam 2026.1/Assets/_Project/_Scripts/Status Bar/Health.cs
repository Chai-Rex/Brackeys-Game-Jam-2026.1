using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Health : Progressive, IDrainable, IRefillable
{
    [SerializeField]private UnityEvent OnDie;

    public void Drain(float amount)
    {
        Current -= amount;
        OnChange?.Invoke();
        if(Current<=0f){OnDie.Invoke();}
    }

    public void Refill(float amount)
    {
        Current += amount;
        if(Current > Initial)
            Current = Initial;
    }
}
