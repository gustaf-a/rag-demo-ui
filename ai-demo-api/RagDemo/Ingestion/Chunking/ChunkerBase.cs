using Microsoft.Extensions.Configuration;
using Shared.Extensions;
using Shared.Models;
using Shared.Extensions;
using Shared.Configuration;
using Shared.Generation.LlmServices;

namespace AiDemos.Api.Ingestion.Chunking;

public class ChunkerBase
{
    protected readonly IngestionOptions _ingestionOptions;
    protected readonly ILlmServiceFactory _llmServiceFactory;

    public ChunkerBase(IConfiguration configuration, ILlmServiceFactory llmServiceFactory)
    {
        _ingestionOptions = configuration.GetSection(IngestionOptions.Ingestion).Get<IngestionOptions>()
            ?? new IngestionOptions();
        _llmServiceFactory = llmServiceFactory;
    }

    public IEnumerable<ContentChunk> GetChunks(IngestDataOptions ingestDataOptions, string content, int wordsPerChunk, int slidingOverLap)
    {
        var resultChunks = new List<ContentChunk>();
        var paragraphs = content.SplitIntoParagraphs(0);

        List<ContentChunk> smallLineBuffer = [];

        foreach (var paragraph in paragraphs)
        {
            var cleanedParagraph = paragraph.Text.CleanText();

            if (cleanedParagraph.GetWordCount() < ingestDataOptions.MergeLineIfFewerWordsThan)
            {
                smallLineBuffer.Add(new ContentChunk
                {
                    Content = cleanedParagraph,
                    EmbeddingContent = cleanedParagraph,
                    StartIndex = paragraph.StartIndex,
                    EndIndex = paragraph.EndIndex
                });
                continue;
            }

            if (cleanedParagraph.GetWordCount() <= wordsPerChunk)
            {
                smallLineBuffer.Add(new ContentChunk
                {
                    Content = cleanedParagraph,
                    EmbeddingContent = cleanedParagraph,
                    StartIndex = paragraph.StartIndex,
                    EndIndex = paragraph.EndIndex
                });

                AddChunks(ingestDataOptions, resultChunks, wordsPerChunk, slidingOverLap, smallLineBuffer);
                smallLineBuffer = [];
                continue;
            }

            var lines = cleanedParagraph.SplitIntoLines(paragraph.StartIndex);

            foreach (var line in lines)
            {
                var cleanedLine = line.Text.CleanText();

                if (cleanedLine.GetWordCount() < ingestDataOptions.MergeLineIfFewerWordsThan)
                {
                    smallLineBuffer.Add(new ContentChunk
                    {
                        Content = cleanedLine,
                        EmbeddingContent = cleanedLine,
                        StartIndex = line.StartIndex,
                        EndIndex = line.EndIndex
                    });
                    continue;
                }

                smallLineBuffer.Add(new ContentChunk
                {
                    Content = cleanedLine,
                    EmbeddingContent= cleanedLine,
                    StartIndex = line.StartIndex,
                    EndIndex = line.EndIndex
                });

                AddChunks(ingestDataOptions, resultChunks, wordsPerChunk, slidingOverLap, smallLineBuffer);

                smallLineBuffer = [];
            }

            if (smallLineBuffer.Any())
            {
                AddChunks(ingestDataOptions, resultChunks, wordsPerChunk, slidingOverLap, smallLineBuffer);
                smallLineBuffer = [];
            }
        }

        return resultChunks;
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

    private static void AddChunks(IngestDataOptions ingestDataOptions, List<ContentChunk> resultChunks, int wordsPerChunk, int slidingOverLap, List<ContentChunk> contentChunks)
    {
        var mergedChunk = MergeChunks(contentChunks);

        if (mergedChunk.Content.GetWordCount() <= wordsPerChunk)
        {
            resultChunks.Add(mergedChunk);
            return;
        }

        var currentSentences = mergedChunk.Content.SplitIntoSentences(mergedChunk.StartIndex);

        foreach (var sentence in currentSentences)
        {
            var cleanedSentence = sentence.Text.CleanText();

            if (cleanedSentence.GetWordCount() <= wordsPerChunk)
            {
                resultChunks.Add(new ContentChunk
                {
                    Content = cleanedSentence,
                    StartIndex = sentence.StartIndex,
                    EndIndex = sentence.EndIndex
                });
                continue;
            }

            var wordChunks = SplitIntoWordChunks(cleanedSentence, wordsPerChunk, slidingOverLap, sentence.StartIndex);

            resultChunks.AddRange(wordChunks);
        }
    }

    private static List<ContentChunk> SplitIntoWordChunks(string content, int wordsPerChunk, int slidingOverLap, int chunkStartingIndex)
    {
        var chunks = new List<ContentChunk>();

        var words = content.SplitIntoWords(chunkStartingIndex);
        if (words.IsNullOrEmpty())
            return chunks;

        int step = wordsPerChunk - slidingOverLap;
        if (step <= 0)
        {
            step = 1;
        }

        int totalWords = words.Count();
        int currentIndex = 0;

        while (currentIndex < totalWords)
        {
            var endIndexNextChunk = currentIndex + wordsPerChunk * 2;
            if (endIndexNextChunk > totalWords)
            {
                var remainingNumberOfWords = totalWords - currentIndex;
                var remainingWords = words.Skip(currentIndex).Take(remainingNumberOfWords);

                var evenlySplitChunks = GetEvenChunks(remainingWords.ToList());

                chunks.AddRange(evenlySplitChunks);
                break;
            }

            var chunkWords = words.Skip(currentIndex).Take(wordsPerChunk).ToList();

            chunks.Add(CreateChunk(chunkWords));

            currentIndex += step;
        }

        return chunks;
    }

    private static IEnumerable<ContentChunk> GetEvenChunks(List<SplitText> words)
    {
        int splitIndex = (int)Math.Ceiling((double)words.Count / 2);

        var chunkWords1 = words.Take(splitIndex).ToList();
        var chunk1 = CreateChunk(chunkWords1);

        var chunkWords2 = words.Skip(splitIndex).ToList();
        var chunk2 = CreateChunk(chunkWords2);

        return new List<ContentChunk> { chunk1, chunk2 };
    }

    private static ContentChunk MergeChunks(List<ContentChunk> contentChunks)
    {
        if (contentChunks.IsNullOrEmpty())
            return null;

        if(contentChunks.Count == 1)
            return contentChunks[0];

        return new ContentChunk
        {
            Content = string.Join(' ', contentChunks.Select(cc => cc.Content)),
            StartIndex = contentChunks.First().StartIndex,
            EndIndex = contentChunks.Last().EndIndex
        };
    }

    private static ContentChunk CreateChunk(List<SplitText> splitTexts)
    {
        if (splitTexts.IsNullOrEmpty())
            return null;

        var content = string.Join(' ', splitTexts.Select(st => st.Text));

        return new ContentChunk
        {
            Content = content,
            EmbeddingContent = content,
            StartIndex = splitTexts.First().StartIndex,
            EndIndex = splitTexts.Last().EndIndex
        };
    }
}