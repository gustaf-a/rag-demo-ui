using RagDemoAPI.Extensions;

namespace RagDemoAPI.Ingestion.Chunking;

public class ChunkingBase
{
    /// <summary>
    /// Splits the input content into chunks based on the specified word count and sliding overlap.
    /// </summary>
    public static IEnumerable<string> GetChunks(string content, int wordsPerChunk, int slidingOverLap)
    {
        var chunks = new List<string>();
        var paragraphs = content.SplitIntoParagraphs();

        foreach (var paragraph in paragraphs)
        {
            var cleanedParagraph = paragraph.CleanText();

            if (cleanedParagraph.GetWordCount() <= wordsPerChunk)
            {
                chunks.Add(cleanedParagraph);
                continue;
            }

            var lines = cleanedParagraph.SplitIntoLines();

            foreach (var line in lines)
            {
                var cleanedLine = line.CleanText();

                if (cleanedLine.GetWordCount() <= wordsPerChunk)
                {
                    chunks.Add(cleanedLine);
                    continue;
                }

                var sentences = cleanedLine.SplitIntoSentences();

                foreach (var sentence in sentences)
                {
                    var cleanedSentence = sentence.CleanText();

                    if (cleanedSentence.GetWordCount() <= wordsPerChunk)
                    {
                        chunks.Add(cleanedSentence);
                        continue;
                    }

                    var wordChunks = SplitIntoWordChunks(cleanedSentence, wordsPerChunk, slidingOverLap);
                    chunks.AddRange(wordChunks);
                }
            }
        }

        return chunks;
    }

    private static IEnumerable<string> SplitIntoWordChunks(string sentence, int wordsPerChunk, int slidingOverLap)
    {
        var chunks = new List<string>();

        var words = sentence.SplitIntoWords();
        if (words.Length == 0)
            return chunks;

        int step = wordsPerChunk - slidingOverLap;
        if (step <= 0)
        {
            // Prevent infinite loop if slidingOverLap >= wordsPerChunk
            step = 1;
        }

        for (int i = 0; i < words.Length; i += step)
        {
            var chunkWords = words.Skip(i).Take(wordsPerChunk).ToArray();
            var chunk = string.Join(" ", chunkWords);
            chunks.Add(chunk);

            if (i + wordsPerChunk >= words.Length)
            {
                break; // Avoid adding empty or incomplete chunks at the end
            }
        }

        return chunks;
    }
}