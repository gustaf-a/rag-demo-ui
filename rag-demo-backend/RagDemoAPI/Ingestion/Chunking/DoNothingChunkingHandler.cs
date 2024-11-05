using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;

public class DoNothingChunkingHandler(IConfiguration configuration, ILlmServiceFactory llmServiceFactory) : ChunkingBase(configuration, llmServiceFactory), IChunkingHandler
{
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
