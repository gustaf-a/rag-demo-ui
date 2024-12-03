
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using RagDemoAPI.Extensions;

namespace RagDemoAPI.Services;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class EmbeddingService(Kernel _kernel) : IEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _textEmbeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

    public async Task<float[]> GetEmbeddings(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        IList<string> contentList = [content];

        var embeddings = await _textEmbeddingService.GenerateEmbeddingsAsync(contentList);

        return embeddings.ConvertToFloatArray();
    }
}
