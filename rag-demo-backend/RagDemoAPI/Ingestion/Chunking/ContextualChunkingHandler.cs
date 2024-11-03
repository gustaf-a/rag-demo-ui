using RagDemoAPI.Configuration;
using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;

namespace RagDemoAPI.Ingestion.Chunking;
public class ContextualChunkingHandler(IConfiguration configuration, ILlmServiceFactory _llmServiceFactory) : IChunkingHandler
{
    private readonly IngestionOptions _ingestionOptions = configuration.GetSection(IngestionOptions.Ingestion).Get<IngestionOptions>() ?? throw new ArgumentNullException(nameof(IngestionOptions));

    public string Name => nameof(ContextualChunkingHandler);

    public bool IsSuitable(IngestDataRequest request, string content)
    {
        if (_ingestionOptions.ContextualRetrievalChunkSize <= content.Length)
            return false;

        return true;
    }

    public async Task<IEnumerable<string>> DoChunking(IngestDataRequest request, string content)
    {
        var contentChunks = GetSimpleChunks(request, content);

        var chunksWithContext = new List<string>();

        var llmService = _llmServiceFactory.Create(request);

        foreach (var contentChunk in contentChunks)
        {
            var contextEnrichedChunk = await GetContextEnrichedChunk(llmService, request, content, contentChunk);

            if (string.IsNullOrWhiteSpace(contextEnrichedChunk))
            {
                continue;
            }

            chunksWithContext.Add(contextEnrichedChunk);
        }

        return chunksWithContext;
    }

    private async Task<string> GetContextEnrichedChunk(ILlmService llmService, IngestDataRequest request, string content, string contentChunk)
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
Please create a short context which helps situate the chunk in the document. Answer ONLY with the short context which explains the context of the chunk.
""";

        //TODO Use caching
        //TODO send in list of only one plugin to llmService
        var completionResponse = await llmService.GetCompletionSimple(contextEnrichedChunkPrompt);

        return completionResponse;
    }

    private IEnumerable<string> GetSimpleChunks(IngestDataRequest request, string content)
    {
        var words = content.Split(' ');
        var chunks = new List<string>();

        for (int i = 0; i < words.Length; i += _ingestionOptions.ContextualRetrievalChunkSize)
        {
            var chunkWords = words.Skip(i).Take(_ingestionOptions.ContextualRetrievalChunkSize).ToArray();
            chunks.Add(string.Join(' ', chunkWords));
        }

        return chunks;
    }


}
