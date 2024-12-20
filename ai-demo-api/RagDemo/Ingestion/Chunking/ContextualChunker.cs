using Microsoft.Extensions.Configuration;
using AiDemos.Api.Generation.LlmServices;
using AiDemos.Api.Models;

namespace AiDemos.Api.Ingestion.Chunking;

public class ContextualChunker(IConfiguration configuration, ILlmServiceFactory llmServiceFactory) : ChunkerBase(configuration, llmServiceFactory), IChunker
{
    public string Name => nameof(ContextualChunker);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        if (_ingestionOptions.ContextualRetrievalWordsPerChunk >= content.Length)
            return false;

        return true;
    }

    public async Task<IEnumerable<ContentChunk>> Execute(IngestDataRequest request, string content)
    {
        var contentChunks = GetChunks(request.IngestDataOptions, content, _ingestionOptions.ContextualRetrievalWordsPerChunk, 0);

        var chunksWithContext = new List<ContentChunk>();

        var llmService = _llmServiceFactory.Create(request);

        foreach (var contentChunk in contentChunks)
        {
            var chunkContext = await GetChunkContext(llmService, request, content, contentChunk.Content);
            if (string.IsNullOrWhiteSpace(chunkContext))
            {
                throw new Exception($"Failed to generate context for chunk: {contentChunk} using LlmService {llmService.GetType()}.");
            }

            contentChunk.EmbeddingContent =
$"""
Context:
{chunkContext}

Information:
{contentChunk}
""";

            chunksWithContext.Add(contentChunk);
        }

        return chunksWithContext;
    }

    private async Task<string> GetChunkContext(ILlmService llmService, IngestDataRequest request, string content, string contentChunk)
    {
        var contextEnrichedChunkPrompt =
$"""
<document> 
{content} 
</document> 
Above is the entire document and below is the chunk within the document which we want to create context for.
<chunk> 
{contentChunk} 
</chunk> 
Please create a short context which helps situate the chunk in the document. Answer ONLY with the short context which explains the context of the chunk and do not repeat the chunk.
""";

        var completionResponse = await llmService.GetCompletionSimple(contextEnrichedChunkPrompt);

        return completionResponse;
    }
}
