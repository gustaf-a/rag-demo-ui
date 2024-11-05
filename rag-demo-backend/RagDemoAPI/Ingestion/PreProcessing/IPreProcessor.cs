namespace RagDemoAPI.Ingestion.PreProcessing;

public interface IPreProcessor
{
    abstract string Name { get; }
    bool IsSuitable(string fileExtension);
    string Execute(string fileContent);
}
