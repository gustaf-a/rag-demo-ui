namespace RagDemoAPI.Services;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddings(string content);
}