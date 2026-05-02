using System;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
    public static IEnumerable<TValue> RandomValues<TKey, TValue>(this IDictionary<TKey, TValue> dict)
    {
        Random rand = new();
        List<TValue> values = dict.Values.ToList();
        int size = dict.Count;
        while(true)
        {
            yield return values[rand.Next(size)];
        }
    }

    public static TValue RandomValue<TKey, TValue>(this IDictionary<TKey, TValue> dict)
    {
        return dict.RandomValues().Take(1).First();
    }
    
    public static T Random<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            throw new ArgumentNullException(nameof(enumerable));
        }

        Random r = new Random();  
        IList<T> list = enumerable as IList<T> ?? enumerable.ToList(); 
        return list.Count == 0 ? default(T) : list[r.Next(0, list.Count)];
    }

}