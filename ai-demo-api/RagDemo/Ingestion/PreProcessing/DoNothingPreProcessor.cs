namespace RagDemoAPI.Ingestion.PreProcessing;

public class DoNothingPreProcessor : IPreProcessor
{
    public string Name => nameof(DoNothingPreProcessor);

    public string Execute(string fileContent)
    {
        return fileContent;
    }

    public bool IsSuitable(string fileExtension)
    {
        return true;
    }
}
