namespace RagDemoAPI.Extensions;

public static class CollectionExtensions
{  
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
    {
        return collection is null || !collection.Any();
    }
    
    public static bool IsNullOrEmpty<T1, T2>(this IDictionary<T1, IEnumerable<T2>> collection)
    {
        return collection is null || !collection.Any();
    }

    public static float[] ConvertToFloatArray(this IList<ReadOnlyMemory<float>> list)
    {
        int totalLength = 0;

        // Calculate the total length for the resulting array
        foreach (var memory in list)
        {
            totalLength += memory.Length;
        }

        float[] array = new float[totalLength];
        int offset = 0;

        // Copy each ReadOnlyMemory<float> to the array
        foreach (var memory in list)
        {
            memory.Span.CopyTo(array.AsSpan(offset));
            offset += memory.Length;
        }

        return array;
    }

    public static IEnumerable<T> TakeLastOrAll<T>(this IList<T> collection, int count)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection), "Source cannot be null.");
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");

        if (count == 0)
            yield break;

        int start = Math.Max(collection.Count() - count, 0);
        for (int i = start; i < collection.Count(); i++)
        {
            yield return collection[i];
        }
    }

    public static T? GetByClassName<T>(this IEnumerable<T> collection, string name) where T : class
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection), "The collection cannot be null.");

        if (name == null)
            throw new ArgumentNullException(nameof(name), "The name cannot be null.");

        return collection.FirstOrDefault(item => item.GetType().Name.Equals(name, StringComparison.Ordinal));
    }
}
