namespace RagDemoAPI.Ingestion.PreProcessing;

public class DoNothingContentPreProcessor : IContentPreProcessor
{
    public string DoPreProcessing(string fileContent)
    {
        return fileContent;
    }

    public bool IsSuitable(string fileExtension)
    {
        return true;
    }
}
