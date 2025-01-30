using Shared.Models;

namespace AiDemos.Api.Ingestion;

public class IndexFileRequestQueueMessage
{
    public IndexFileRequestQueueMessage()
    {
    }

    public string FileUri { get; set; }
    public string FileName { get; set; }
    public EmbeddingMetaData Metadata { get; set; }
    public IngestDataRequest IngestDataRequest { get; set; }
}