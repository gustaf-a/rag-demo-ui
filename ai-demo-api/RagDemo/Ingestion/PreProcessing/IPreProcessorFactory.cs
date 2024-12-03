namespace AiDemos.Api.Ingestion.PreProcessing;

public interface IPreProcessorFactory
{
    IPreProcessor Create(string filePath);
}
