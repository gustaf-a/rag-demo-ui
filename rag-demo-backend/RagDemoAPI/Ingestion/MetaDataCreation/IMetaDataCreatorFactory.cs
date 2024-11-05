using RagDemoAPI.Ingestion.MetaDataCreation;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.PreProcessing;

public interface IMetaDataCreatorFactory
{
    IMetaDataCreator Create(IngestDataRequest request, string filePath, string content);
}
