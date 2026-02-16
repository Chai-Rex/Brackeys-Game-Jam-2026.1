using UnityEngine;

public static class TimeExtension
{
    static MyEvent<float> onTimeScaleChanged = new();

    static public MyEvent<float> OnTimeScaleChanged => onTimeScaleChanged;

    public static float TimeScale
    {
        get => Time.timeScale;

        set {
            value = Mathf.Max(value, 0);   

            if (Time.timeScale == value)
                return;

            Time.timeScale = value;
            OnTimeScaleChanged.Invoke(value);
        }
    }
}
