using AiDemos.Api.Models;

namespace AiDemos.Api.Ingestion.Chunking;

public interface IChunker
{
    abstract string Name { get; }
    bool IsSuitable(IngestDataRequest request, string content);
    Task<IEnumerable<ContentChunk>> Execute(IngestDataRequest request, string content);
}
