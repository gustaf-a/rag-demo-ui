using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;


public class SlidingWindowChunker(IConfiguration configuration, ILlmServiceFactory llmServiceFactory) : ChunkerBase(configuration, llmServiceFactory), IChunker
{
    public string Name => nameof(SlidingWindowChunker);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        return true;
    }

    public async Task<IEnumerable<string>> DoChunking(IngestDataRequest request, string content)
    {
        var contentChunks = GetChunks(request.IngestDataOptions, content, _ingestionOptions.SlidingWindowWordsPerChunk, _ingestionOptions.SlidingWindowOverlapWords);

        return await Task.FromResult(contentChunks);
    }
}
