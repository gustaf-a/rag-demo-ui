using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion
{
    public interface IIngestionHandler
    {
        IEnumerable<string> GetChunkerNames();
        Task IngestDataFromFolder(IngestDataRequest request);
    }
}