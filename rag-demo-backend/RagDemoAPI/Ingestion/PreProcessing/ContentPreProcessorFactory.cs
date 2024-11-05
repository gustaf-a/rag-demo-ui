namespace RagDemoAPI.Ingestion.PreProcessing;

public class ContentPreProcessorFactory(IEnumerable<IContentPreProcessor> _contentPreProcessors) : IContentPreProcessorFactory
{
    public IContentPreProcessor Create(string filePath)
    {
        var fileExtension = Path.GetExtension(filePath);

        var usableChunkers = _contentPreProcessors.Where(ch => ch.IsSuitable(fileExtension));

        return usableChunkers.First();
    }
}
