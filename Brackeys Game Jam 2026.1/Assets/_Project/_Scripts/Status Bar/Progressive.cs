using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Progressive : MonoBehaviour
{
    [SerializeField]private float _initial;
    private float _current;

    public float Current{
        get
        {
            return _current;
        }
        set
        {
            _current = value;
            OnChange?.Invoke();
        }
    }

    public float Initial => _initial;
    public float Ratio => _current / _initial;
    public Action OnChange;

    private void Awake() => _current = _initial;

    //Set new inital value using increment and scale current value accordingly
    public void SetNewInitial(SkillUpgradeSO skillUpgrade)
    {
        float increment = skillUpgrade.SkillUpgrades[0].UpgradeAmount;
        
        float newInitial = increment + _initial;
        float newRatio = newInitial / _initial;

        _current *= newRatio;
        _initial = newInitial;
        
        OnChange?.Invoke();
    }
}
