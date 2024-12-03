using RagDemoAPI.Extensions;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.FileReaders;

public abstract class FileReaderBase(Dictionary<string, string> _metaDataTags)
{
    protected EmbeddingMetaData CreateMetaData(string source, string uri)
    {
        return new EmbeddingMetaData
        {
            Source = source,
            CreatedDateTime = DateTime.UtcNow,
            Tags = _metaDataTags.IsNullOrEmpty() ? [] : _metaDataTags,
            Uri = uri
        };
    }
}
