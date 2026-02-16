using System;
using System.Collections.Generic;
using System.Linq;


public static class LinkExtension
{
    public static T[] Clone<T>(this T[] array) => array.ToArray();
    public static List<T> Clone<T>(this List<T> list) => list.ToList();
    public static HashSet<T> Clone<T>(this HashSet<T> set) => set.ToHashSet();
    public static Dictionary<T1, T2> Clone<T1, T2>(this Dictionary<T1, T2> dico)
    {
        Dictionary<T1, T2> clone = new();

        foreach (KeyValuePair<T1, T2> kv in dico)
            clone.Add(kv.Key, kv.Value);

        return clone;
    }
    public static IEnumerable<T> Clone<T>(this IEnumerable<T> ienum)
    {
        List<T> list = new();

        foreach (T item in ienum)
            list.Add(item);

        return list;
    }

    public static bool Contains<T>(this IEnumerable<T> ienum, Predicate<T> predicate)
    {
        foreach (T item in ienum)
            if (predicate(item))
                return true;

        return false;
    }

    public static bool TryFind<T>(this IEnumerable<T> ienum, Predicate<T> predicate, out T item)
    {
        foreach (T crtItem in ienum)
            if (predicate(crtItem))
            {
                item = crtItem;
                return true;
            }

        item = default(T);
        return false;
    }

    public static bool Compare<T>(this IEnumerable<T> ienum1, IEnumerable<T> ienum2)
    {
        if (ienum1 == null &&
            ienum2 == null)
            return true;

        if (ienum1 == null ||
            ienum2 == null)
            return false;

        T[] array1 = ienum1.ToArray();
        T[] array2 = ienum2.ToArray();

        if (array1.Length != array2.Length)
            return false;

        for (int i = 0; i < array1.Length; i++)
            if (!Equals(array1[i], array2[i]))
                return false;

        return true;
    }

    public static int RemoveAll<T>(this List<T> list, T item)
        => list.RemoveAll((i) => i.Equals(item));

    public static int IndexOf<T>(this T[] array, T item)
    {
        for (int i = 0; i < array.Length; i++)
            if (Equals(array[i], item))
                return i;

        return -1;
    }
}
