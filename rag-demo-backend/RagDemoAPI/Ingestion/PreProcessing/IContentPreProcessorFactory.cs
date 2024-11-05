namespace RagDemoAPI.Ingestion.PreProcessing
{
    public interface IContentPreProcessorFactory
    {
        IContentPreProcessor Create(string filePath);
    }
}
