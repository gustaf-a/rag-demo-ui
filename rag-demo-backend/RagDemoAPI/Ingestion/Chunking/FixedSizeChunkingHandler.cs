using RagDemoAPI.Configuration;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;
public class FixedSizeChunkingHandler(IConfiguration configuration) : IChunkingHandler
{
    private readonly IngestionOptions _ingestionOptions = configuration.GetSection(IngestionOptions.Ingestion).Get<IngestionOptions>() ?? throw new ArgumentNullException(nameof(IngestionOptions));

    public string Name => nameof(FixedSizeChunkingHandler);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        return true;
    }

    public async Task<IEnumerable<string>> DoChunking(IngestDataRequest request, string content)
    {
        int chunkSize = _ingestionOptions.FixedSizeChunkingChunkSize;
        var chunks = new List<string>();

        for (int i = 0; i < content.Length; i += chunkSize)
        {
            chunks.Add(content.Substring(i, Math.Min(chunkSize, content.Length - i)));
        }

        return await Task.FromResult(chunks);
    }
}
