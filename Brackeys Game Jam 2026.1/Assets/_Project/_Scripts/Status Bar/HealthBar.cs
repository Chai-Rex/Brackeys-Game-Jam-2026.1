using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]private Progressive _health;
    [SerializeField]private Image _frontDisplay;
    [SerializeField]private Image _backDisplay;

    [SerializeField]private float chipSpeed = 2f;
    private float lerpTimer = 0f;
    private float valueToLerp;

    private void OnEnable() => _health.OnChange += updateBAR;
    private void OnDisable() => _health.OnChange -= updateBAR;

    private void updateBAR(){
        lerpTimer = 0f;
        float fillF = _frontDisplay.fillAmount;
        float fillB = _backDisplay.fillAmount;
        if(fillB > _health.Ratio){
            _frontDisplay.fillAmount = _health.Ratio;
            _backDisplay.color = Color.blue;
            StartCoroutine(Lerp(fillB, _health.Ratio, "Drain"));
        }
        else if(fillF < _health.Ratio){
            _backDisplay.color = Color.green;
            _backDisplay.fillAmount = _health.Ratio;
            StartCoroutine(Lerp(fillF, _backDisplay.fillAmount, "Refill"));
        }
    }
    IEnumerator Lerp(float start, float end, string effectType)
    {
        float timeElapsed = 0;
        while (timeElapsed < 1f)
        {
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            valueToLerp = Mathf.Lerp(start, end, percentComplete);
            if(effectType=="Drain"){
                _backDisplay.fillAmount = valueToLerp;
            }else{
                _frontDisplay.fillAmount = valueToLerp;
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        valueToLerp = end;
        if(effectType=="Drain"){
            _backDisplay.fillAmount = valueToLerp;
        }else{
            _frontDisplay.fillAmount = valueToLerp;
        }

    }
}
