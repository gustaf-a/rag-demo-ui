namespace AiDemos.Api.Ingestion.Chunking;

public interface IChunkerFactory
{
    IEnumerable<string> GetChunkerNames();
    IChunker Create(Models.IngestDataRequest request, string filePath, string fileContent);
}
