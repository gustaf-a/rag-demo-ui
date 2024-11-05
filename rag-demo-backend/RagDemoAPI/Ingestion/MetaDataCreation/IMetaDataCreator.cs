using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.MetaDataCreation;

public interface IMetaDataCreator
{
    abstract string Name { get; }
    bool IsSuitable(IngestDataRequest request, string filePath, string content);
    public EmbeddingMetaData Execute(IngestDataRequest request, string filePath, string content);
}
