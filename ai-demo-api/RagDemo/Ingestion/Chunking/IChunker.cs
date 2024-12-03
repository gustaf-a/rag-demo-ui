using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;

public interface IChunker
{
    abstract string Name { get; }
    bool IsSuitable(IngestDataRequest request, string content);
    Task<IEnumerable<string>> Execute(IngestDataRequest request, string content);
}
