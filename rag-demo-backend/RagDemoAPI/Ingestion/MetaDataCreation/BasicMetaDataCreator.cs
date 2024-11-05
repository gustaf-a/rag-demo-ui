using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.MetaDataCreation;

public class BasicMetaDataCreator : IMetaDataCreator
{
    public string Name => nameof(BasicMetaDataCreator);

    public EmbeddingMetaData Execute(IngestDataRequest request, string filePath, string content)
    {
        return new EmbeddingMetaData
        {
            CreatedDateTime = DateTime.UtcNow,
            Source = request.FolderPath,
            Uri = filePath,
            Category = ""
        };
    }

    public bool IsSuitable(IngestDataRequest request, string filePath, string content)
    {
        return true;
    }
}
