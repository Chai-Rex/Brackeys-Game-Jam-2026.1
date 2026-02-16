using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class TimeScaleSlider : MonoBehaviour
{
    void Awake()
    {
        Slider slider = GetComponent<Slider>();
        TimeExtension.TimeScale = slider.value;
        slider.onValueChanged.AddListener((float value) => { TimeExtension.TimeScale = value; });
    }
}
