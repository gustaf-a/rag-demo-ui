using RagDemoAPI.Configuration;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;

public class SlidingWindowChunkingHandler(IConfiguration configuration) : ChunkingBase, IChunkingHandler
{
    private readonly IngestionOptions _ingestionOptions = configuration.GetSection(IngestionOptions.Ingestion).Get<IngestionOptions>() ?? throw new ArgumentNullException(nameof(IngestionOptions));

    public string Name => nameof(SlidingWindowChunkingHandler);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        return true;
    }

    public async Task<IEnumerable<string>> DoChunking(IngestDataRequest request, string content)
    {
        var contentChunks = GetChunks(content, _ingestionOptions.SlidingWindowWordsPerChunk, _ingestionOptions.SlidingWindowOverlapWords);

        return await Task.FromResult(contentChunks);
    }
}
