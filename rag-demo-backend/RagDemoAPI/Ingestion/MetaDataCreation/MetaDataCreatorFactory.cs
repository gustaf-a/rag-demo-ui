using RagDemoAPI.Ingestion.PreProcessing;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.MetaDataCreation;

public class MetaDataCreatorFactory(IEnumerable<IMetaDataCreator> _metaDataCreators) : IMetaDataCreatorFactory
{
    public IMetaDataCreator Create(IngestDataRequest request, string filePath, string content)
    {
        var suitableCreators = _metaDataCreators.Where(ch => ch.IsSuitable(request, filePath, content));

        return suitableCreators.First();
    }
}
