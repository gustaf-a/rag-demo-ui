using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion
{
    public interface IIngestionHandler
    {
        IEnumerable<string> GetChunkingHandlerNames();
        Task IngestDataFromFolderAsync(IngestDataRequest request);
    }
}