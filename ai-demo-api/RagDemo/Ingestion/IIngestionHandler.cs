using AiDemos.Api.Models;

namespace AiDemos.Api.Ingestion
{
    public interface IIngestionHandler
    {
        IEnumerable<string> GetChunkerNames();
        Task IngestData(IngestDataRequest request);
    }
}