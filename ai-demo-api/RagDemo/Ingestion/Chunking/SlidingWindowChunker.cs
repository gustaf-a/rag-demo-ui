using Microsoft.Extensions.Configuration;
using AiDemos.Api.Generation.LlmServices;
using AiDemos.Api.Models;

namespace AiDemos.Api.Ingestion.Chunking;

public class SlidingWindowChunker(IConfiguration configuration, ILlmServiceFactory llmServiceFactory) : ChunkerBase(configuration, llmServiceFactory), IChunker
{
    public string Name => nameof(SlidingWindowChunker);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        return true;
    }

    public async Task<IEnumerable<ContentChunk>> Execute(IngestDataRequest request, string content)
    {
        var contentChunks = GetChunks(request.IngestDataOptions, content, _ingestionOptions.SlidingWindowWordsPerChunk, _ingestionOptions.SlidingWindowOverlapWords);

        return await Task.FromResult(contentChunks);
    }
}
