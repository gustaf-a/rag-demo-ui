using Shared.Models;

namespace Shared.Extensions;

public static class ContentChunkExtensions
{
    public static int GetWordCount(this List<ContentChunk> contentChunks)
    {
        if (contentChunks.IsNullOrEmpty())
            return 0;

        var wordCount = contentChunks.Sum(cc => cc.Content.GetWordCount());

        return wordCount;
    }

    public static string GetContents(this IEnumerable<ContentChunk> contentChunks)
    {
        if (contentChunks.IsNullOrEmpty())
            return string.Empty;

        var contents = string.Join(" ", contentChunks.Select(cc => cc.Content));

        return contents;
    }
}