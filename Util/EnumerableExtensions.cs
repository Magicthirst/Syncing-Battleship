namespace Util;

public static class EnumerableExtensions
{
    public static void RemoveWhere<TKey, TValue>
    (
        this IDictionary<TKey, TValue> dictionary,
        Func<TKey, TValue, bool> predicate
    )
    {
        foreach (var (key, value) in dictionary)
        {
            if (predicate(key, value))
            {
                dictionary.Remove(key);
            }
        }
    }

    public static bool TryGet<T>
    (
        this IList<T> list,
        out T value,
        Func<T, bool> predicate
    )
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        value = list.FirstOrDefault(predicate);
#pragma warning restore CS8601 // Possible null reference assignment.
        return value != null;
    }
}
