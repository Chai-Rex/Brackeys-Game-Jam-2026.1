using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RandomExtension
{
    public static bool FlipCoin() => Random.value < 0.5f;
    
    public static T PickRandom<T>(this T[] array) => array.Length == 0 ? default : array[(int)(Random.value * array.Length)];
    public static T PickRandom<T>(this List<T> list) => list.Count == 0 ? default : list[(int)(Random.value * list.Count)];
    public static T PickRandom<T>(this IEnumerable<T> ienum) => ienum.ToArray().PickRandom();

    public static float RandomInRange(this Vector2 range)
    {
        if (range.x > range.y)
            range = new Vector2(range.y, range.x);

        return Random.Range(range.x, range.y);
    }

    public static int RandomInRange(this Vector2Int range, bool minInclusive = true, bool maxInclusive = true)
    {
        if (!minInclusive)
            range.x++;

        if (maxInclusive)
            range.y++;

        if (range.x >= range.y)
            throw new System.Exception("RandomExtension.RandomInRange : Invalid range.");

        return Random.Range(range.x, range.y);
    }
    
    public static Vector2 RandomVector2() => new Vector2(Random.value, Random.value);
    public static Vector3 RandomVector3() => new Vector3(Random.value, Random.value, Random.value);
    public static Vector2 RandomVector2(float min, float max) => new Vector2(Random.Range(min, max), Random.Range(min, max));
    public static Vector3 RandomVector3(float min, float max) => new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
    
    public static Vector2 RandomPointInCircle(float radius = 1)
    {
        while (true)
        {
            Vector2 pnt = RandomVector2(-1, 1);
            
            if (pnt.sqrMagnitude <= 1)
                return pnt * radius;
        }
    }
    public static Vector3 RandomPointInSphere(float radius = 1)
    {
        while (true)
        {
            Vector3 pnt = RandomVector3(-1, 1);
            
            if (pnt.sqrMagnitude <= 1)
                return pnt * radius;
        }
    }

    // Correct Mathf.PerlinNoise1D drift : min = -0.9305f, max = 2-0.9305f
    public static Vector2 PerlinVector2(Vector2 position, float min = -1, float max = 1) 
        => Vector2.one * min + new Vector2(Mathf.PerlinNoise1D(position.x), Mathf.PerlinNoise1D(position.y)) * (max - min);

    public static Vector3 PerlinVector3(Vector3 position, float min = -1, float max = 1)
        => Vector3.one * min + new Vector3(Mathf.PerlinNoise1D(position.x), Mathf.PerlinNoise1D(position.y), Mathf.PerlinNoise1D(position.z)) * (max - min);

}


