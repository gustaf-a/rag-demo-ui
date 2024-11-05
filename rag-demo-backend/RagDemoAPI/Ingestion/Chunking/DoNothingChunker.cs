using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;

public class DoNothingChunker(IConfiguration configuration, ILlmServiceFactory llmServiceFactory) : ChunkerBase(configuration, llmServiceFactory), IChunker
{
    public string Name => nameof(DoNothingChunker);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        return content.Length >= _ingestionOptions.MaxChunkWords;
    }

    public Task<IEnumerable<string>> Execute(IngestDataRequest request, string content)
    {
        IEnumerable<string> chunks = new List<string>
        {
            content
        };

        return Task.FromResult(chunks);
    }
}
