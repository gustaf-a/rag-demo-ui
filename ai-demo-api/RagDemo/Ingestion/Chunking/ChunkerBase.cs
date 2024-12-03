using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;

public class ChunkerBase
{
    protected readonly IngestionOptions _ingestionOptions;
    protected readonly ILlmServiceFactory _llmServiceFactory;

    public ChunkerBase(IConfiguration configuration, ILlmServiceFactory llmServiceFactory)
    {
        _ingestionOptions = configuration.GetSection(IngestionOptions.Ingestion).Get<IngestionOptions>()
            ?? throw new ArgumentNullException(nameof(IngestionOptions));
        _llmServiceFactory = llmServiceFactory;
    }

    public IEnumerable<string> GetChunks(IngestDataOptions ingestDataOptions, string content, int wordsPerChunk, int slidingOverLap)
    {
        var chunks = new List<string>();
        var paragraphs = content.SplitIntoParagraphs();

        List<string> smallLineBuffer = [];

        foreach (var paragraph in paragraphs)
        {
            var cleanedParagraph = paragraph.CleanText();

            if (cleanedParagraph.GetWordCount() < ingestDataOptions.MergeLineIfFewerWordsThan)
            {
                smallLineBuffer.Add(cleanedParagraph);
                continue;
            }

            if (cleanedParagraph.GetWordCount() <= wordsPerChunk)
            {
                AddChunksFromLine(ingestDataOptions, chunks, wordsPerChunk, slidingOverLap, string.Join(" ", smallLineBuffer) + cleanedParagraph);
                smallLineBuffer = [];
                continue;
            }

            var lines = cleanedParagraph.SplitIntoLines();

            foreach (var line in lines)
            {
                var cleanedLine = line.CleanText();
                int wordCount = cleanedLine.GetWordCount();

                if (wordCount < ingestDataOptions.MergeLineIfFewerWordsThan)
                {
                    smallLineBuffer.Add(cleanedLine);
                    continue;
                }

                if (smallLineBuffer.Any())
                {
                    AddChunksFromLine(ingestDataOptions, chunks, wordsPerChunk, slidingOverLap, string.Join(" ", smallLineBuffer));

                    smallLineBuffer = [];
                }

                AddChunksFromLine(ingestDataOptions, chunks, wordsPerChunk, slidingOverLap, cleanedLine);
            }

            if (smallLineBuffer.Any())
            {
                AddChunksFromLine(ingestDataOptions, chunks, wordsPerChunk, slidingOverLap, string.Join(" ", smallLineBuffer));
            }
        }

        return chunks;
    }

    private static void AppendToLastChunk(List<string> chunks, List<string> smallLineBuffer)
    {
        var contentToAdd = string.Join(" ", smallLineBuffer);

        if (chunks.IsNullOrEmpty())
        {
            chunks = [contentToAdd];
        }
        else
        {
            int lastIndex = chunks.Count - 1;
            chunks[lastIndex] += " " + contentToAdd;
        }
    }

    private static void AddChunksFromLine(IngestDataOptions ingestDataOptions, List<string> chunks, int wordsPerChunk, int slidingOverLap, string contentLine)
    {
        if (contentLine.GetWordCount() <= wordsPerChunk)
        {
            chunks.Add(contentLine);
            return;
        }

        var currentSentences = contentLine.SplitIntoSentences();

        foreach (var sentence in currentSentences)
        {
            var cleanedSentence = sentence.CleanText();

            if (cleanedSentence.GetWordCount() <= wordsPerChunk)
            {
                chunks.Add(cleanedSentence);
                continue;
            }

            var wordChunks = SplitIntoWordChunks(cleanedSentence, wordsPerChunk, slidingOverLap);

            chunks.AddRange(wordChunks.Where(wc => !string.IsNullOrWhiteSpace(wc)));
        }
    }

    private static List<string> SplitIntoWordChunks(string content, int wordsPerChunk, int slidingOverLap)
    {
        var chunks = new List<string>();

        var words = content.SplitIntoWords();
        if (words.Length == 0)
            return chunks;

        int step = wordsPerChunk - slidingOverLap;
        if (step <= 0)
        {
            step = 1;
        }

        int totalWords = words.Length;
        int currentIndex = 0;

        while (currentIndex < totalWords)
        {
            var endIndexNextChunk = currentIndex + wordsPerChunk * 2;
            if (endIndexNextChunk > totalWords)
            {
                var remainingNumberOfWords = totalWords - currentIndex;
                var remainingWords = words.Skip(currentIndex).Take(remainingNumberOfWords);

                chunks.AddRange(GetEvenChunks(remainingWords));
                break;
            }

            var chunkWords = words.Skip(currentIndex).Take(wordsPerChunk).ToArray();

            chunks.Add(string.Join(" ", chunkWords));

            currentIndex += step;
        }

        return chunks;
    }

    private static IEnumerable<string> GetEvenChunks(IEnumerable<string> words)
    {
        var wordList = words.ToList();

        int splitIndex = (int)Math.Ceiling((double)wordList.Count / 2);

        var chunkWords1 = wordList.Take(splitIndex);
        var chunk1 = string.Join(" ", chunkWords1);

        var chunkWords2 = wordList.Skip(splitIndex);
        var chunk2 = string.Join(" ", chunkWords2);

        return new List<string> { chunk1, chunk2 };
    }
}