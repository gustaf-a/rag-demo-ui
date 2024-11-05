using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;


public class SlidingWindowChunkingHandler(IConfiguration configuration, ILlmServiceFactory llmServiceFactory) : ChunkingBase(configuration, llmServiceFactory), IChunkingHandler
{
    public string Name => nameof(SlidingWindowChunkingHandler);

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
