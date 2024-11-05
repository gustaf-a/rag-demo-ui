namespace RagDemoAPI.Ingestion.Chunking;

public interface IChunkingHandlerFactory
{
    IEnumerable<string> GetChunkingHandlerNames();
    IChunkingHandler Create(Models.IngestDataRequest request, string filePath, string fileContent);
}
