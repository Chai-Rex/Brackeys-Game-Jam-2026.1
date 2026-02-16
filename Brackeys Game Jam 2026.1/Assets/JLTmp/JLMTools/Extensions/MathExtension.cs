using UnityEngine;


public static class MathExtension
{
    public static float Lerp(this Vector2 vector2, float t) => Mathf.Lerp(vector2.x, vector2.y, t);

    public static Vector2 SLerp(Vector2 a, Vector2 b, float t)
    {
        t = Mathf.Clamp01(t);

        float mag = Mathf.Lerp(a.magnitude, b.magnitude, t);

        if (a.sqrMagnitude < Mathf.Epsilon || b.sqrMagnitude < Mathf.Epsilon)
            return Vector2.Lerp(a, b, t);

        float angleA = Mathf.Atan2(a.y, a.x);
        float angleB = Mathf.Atan2(b.y, b.x);

        float angle = Mathf.LerpAngle(angleA * Mathf.Rad2Deg, 
                                    angleB * Mathf.Rad2Deg, 
                                    t) * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * mag;
    }
}


