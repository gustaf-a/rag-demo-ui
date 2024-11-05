namespace RagDemoAPI.Extensions;

public static class CollectionExtensions
{
    public static bool IsNullOrEmpty(this IEnumerable<object> collection)
    {
        return collection is null || !collection.Any();
    }

    public static bool IsNullOrEmpty(this IDictionary<string, string> collection)
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
}
