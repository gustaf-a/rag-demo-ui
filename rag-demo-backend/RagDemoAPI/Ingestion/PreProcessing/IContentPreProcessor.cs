namespace RagDemoAPI.Ingestion.PreProcessing;

public interface IContentPreProcessor
{
    bool IsSuitable(string fileExtension);
    string DoPreProcessing(string fileContent);
}
