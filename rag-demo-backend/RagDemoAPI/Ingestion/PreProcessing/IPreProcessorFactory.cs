namespace RagDemoAPI.Ingestion.PreProcessing;

public interface IPreProcessorFactory
{
    IPreProcessor Create(string filePath);
}
