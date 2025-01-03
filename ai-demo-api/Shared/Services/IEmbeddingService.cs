namespace Shared.Services;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddings(string content);
}