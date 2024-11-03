using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;

public interface IChunkingHandler
{
    abstract string Name { get; }
    bool IsSuitable(IngestDataRequest request, string content);
    Task<IEnumerable<string>> DoChunking(IngestDataRequest request, string content);
}
