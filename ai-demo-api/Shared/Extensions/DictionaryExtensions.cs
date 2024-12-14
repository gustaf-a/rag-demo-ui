namespace Shared.Extensions;

public static class DictionaryExtensions
{
    public static void AddOrUpdateRange<TKey, TValue>(this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source)
    {
        if (source == null)
            return;

        target ??= [];

        foreach (var kvp in source)
        {
            // Overwrite if key already exists, add if it doesn't.
            target[kvp.Key] = kvp.Value;
        }
    }

    public static bool ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        return keys.All(key => dictionary.ContainsKey(key));
    }
}
