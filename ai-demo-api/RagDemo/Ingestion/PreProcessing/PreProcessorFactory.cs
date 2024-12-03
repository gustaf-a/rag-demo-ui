namespace AiDemos.Api.Ingestion.PreProcessing;

public class PreProcessorFactory(IEnumerable<IPreProcessor> _contentPreProcessors) : IPreProcessorFactory
{
    public IPreProcessor Create(string filePath)
    {
        var fileExtension = Path.GetExtension(filePath);

        var usableChunkers = _contentPreProcessors.Where(ch => ch.IsSuitable(fileExtension));

        return usableChunkers.First();
    }
}
