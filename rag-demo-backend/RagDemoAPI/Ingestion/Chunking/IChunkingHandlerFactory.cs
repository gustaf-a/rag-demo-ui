namespace RagDemoAPI.Ingestion.Chunking;

public interface IChunkingHandlerFactory
{
    IEnumerable<string> GetChunkingHandlerNames();
    IChunkingHandler CreateChunkingHandler(Models.IngestDataRequest request, string filePath, string fileContent);
}
