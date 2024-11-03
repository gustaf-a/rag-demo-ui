namespace RagDemoAPI.Services;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingsAsync(string content);
}