using Microsoft.Extensions.Configuration;
using Shared.Models;
using Shared.Generation.LlmServices;

namespace AiDemos.Api.Ingestion.Chunking;

public class DoNothingChunker(IConfiguration configuration, ILlmServiceFactory llmServiceFactory) : ChunkerBase(configuration, llmServiceFactory), IChunker
{
    public string Name => nameof(DoNothingChunker);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        return content.Length >= _ingestionOptions.MaxChunkWords;
    }

    public Task<IEnumerable<ContentChunk>> Execute(IngestDataRequest request, string content)
    {
        IEnumerable<ContentChunk> chunks = new List<ContentChunk>
        {
            new() {
                Content = content,
                EmbeddingContent = content,
                StartIndex = 0,
                EndIndex = content.Length - 1
            }
        };

        return Task.FromResult(chunks);
    }
}
