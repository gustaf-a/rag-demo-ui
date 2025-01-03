using Shared.Models;

namespace AiDemos.Api.Ingestion.Chunking;

public interface IChunkerFactory
{
    IEnumerable<string> GetChunkerNames();
    IChunker Create(IngestDataRequest request, string filePath, string fileContent);
}
