using RagDemoAPI.Configuration;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;

public class SlidingWindowChunkingHandler(IConfiguration configuration) : IChunkingHandler
{
    private readonly IngestionOptions _ingestionOptions = configuration.GetSection(IngestionOptions.Ingestion).Get<IngestionOptions>() ?? throw new ArgumentNullException(nameof(IngestionOptions));

    public string Name => nameof(SlidingWindowChunkingHandler);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        return true;
    }

    public async Task<IEnumerable<string>> DoChunking(IngestDataRequest request, string content)
    {
        int chunkSize = _ingestionOptions.SlidingWindowChunkSize;
        int overlapSize = _ingestionOptions.SlidingWindowOverlapSize;
        var chunks = new List<string>();

        for (int i = 0; i < content.Length; i += (chunkSize - overlapSize))
        {
            chunks.Add(content.Substring(i, Math.Min(chunkSize, content.Length - i)));
        }

        return await Task.FromResult(chunks);
    }
}
