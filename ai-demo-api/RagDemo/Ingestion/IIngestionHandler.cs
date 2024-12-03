using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion
{
    public interface IIngestionHandler
    {
        IEnumerable<string> GetChunkerNames();
        Task IngestData(IngestDataRequest request);
    }
}