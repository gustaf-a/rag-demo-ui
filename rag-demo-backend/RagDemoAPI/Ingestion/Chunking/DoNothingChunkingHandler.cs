using RagDemoAPI.Configuration;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;

public class DoNothingChunkingHandler(IConfiguration configuration) : ChunkingBase, IChunkingHandler
{
    private readonly IngestionOptions _ingestionOptions = configuration.GetSection(IngestionOptions.Ingestion).Get<IngestionOptions>() ?? throw new ArgumentNullException(nameof(IngestionOptions));

    public string Name => nameof(DoNothingChunkingHandler);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        return content.Length >= _ingestionOptions.MaxChunkWords;
    }

    public Task<IEnumerable<string>> DoChunking(IngestDataRequest request, string content)
    {
        IEnumerable<string> chunks = new List<string>
        {
            content
        };

        return Task.FromResult(chunks);
    }
}
