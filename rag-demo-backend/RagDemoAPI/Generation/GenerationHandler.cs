using RagDemoAPI.Extensions;
using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;
using RagDemoAPI.Retrieval;

namespace RagDemoAPI.Generation;

public class GenerationHandler(ILogger<GenerationHandler> _logger, ILlmServiceFactory _llmServiceFactory, IRetrievalHandler _retrievalHandler) : IGenerationHandler
{
    public async Task<ChatResponse> GetChatResponse(ChatRequest chatRequest)
    {
        ArgumentNullException.ThrowIfNull(chatRequest);
        if (chatRequest.ChatMessages.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(chatRequest.ChatMessages));

        var llmService = _llmServiceFactory.Create(chatRequest.ChatOptions);

        var chatResponse = await llmService.GetChatResponse(chatRequest.ChatMessages, chatRequest.ChatOptions);

        return chatResponse;
    }

    public async Task<ChatResponse> GetRetrievalAugmentedChatResponse(ChatRequest chatRequest)
    {
        ArgumentNullException.ThrowIfNull(chatRequest);
        ArgumentNullException.ThrowIfNull(chatRequest.SearchOptions);

        if (chatRequest.ChatMessages.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(ChatRequest.ChatMessages));

        if (chatRequest.ProvidedDocumentSources.IsNullOrEmpty())
        {
            var retrievedContextSources = await _retrievalHandler.RetrieveContextForQuery(chatRequest);
            if (retrievedContextSources.IsNullOrEmpty())
            {
                return new ChatResponse($"Failed to retrieve contextSources for data retrieval. Ensure search options are correct and database has data.");
            }

            chatRequest.ProvidedDocumentSources = retrievedContextSources;
        }

        var chatResponse = await GetChatResponseInternal(chatRequest);

        return chatResponse;
    }

    private async Task<ChatResponse> GetChatResponseInternal(ChatRequest chatRequest)
    {
        var llmService = _llmServiceFactory.Create(chatRequest.ChatOptions);

        return await llmService.GetChatResponse(chatRequest.ChatMessages, chatRequest.ProvidedDocumentSources, chatRequest.ChatOptions);
    }
}
