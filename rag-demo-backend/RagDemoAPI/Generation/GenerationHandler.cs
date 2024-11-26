﻿using RagDemoAPI.Extensions;
using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;
using RagDemoAPI.Retrieval;
using System.Text.Json;

namespace RagDemoAPI.Generation;

public class GenerationHandler(ILogger<GenerationHandler> _logger, ILlmServiceFactory _llmServiceFactory, IRetrievalHandler _retrievalHandler) : IGenerationHandler
{
    public async Task<ChatResponse> GetChatResponse(ChatRequest chatRequest)
    {
        ArgumentNullException.ThrowIfNull(chatRequest);

        if (chatRequest.ChatMessages.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(ChatRequest.ChatMessages));

        chatRequest.ChatOptions ??= new ChatOptions();

        if (chatRequest.SearchOptions != null
            && chatRequest.ProvidedDocumentSources.IsNullOrEmpty())
        {
            var retrievedContextSources = await _retrievalHandler.RetrieveContextForQuery(chatRequest);
            if (retrievedContextSources.IsNullOrEmpty())
            {
                return new ChatResponse($"Failed to retrieve contextSources for data retrieval. Ensure search options are correct and database has data.");
            }

            chatRequest.ProvidedDocumentSources = retrievedContextSources;
        }

        var llmService = _llmServiceFactory.Create(chatRequest.ChatOptions);

        var chatResponse = await llmService.GetChatResponse(chatRequest.ChatMessages, chatRequest.ProvidedDocumentSources, chatRequest.ChatOptions);

        var chatHistoryJson = JsonSerializer.Serialize(chatResponse.ChatHistory);

        return chatResponse;
    }

    public async Task<ChatResponse> ContinueChatResponse(ContinueChatRequest continueChatRequest)
    {
        ArgumentNullException.ThrowIfNull(continueChatRequest.PreviousChatHistoryJson);

        if (continueChatRequest.ChatRequest is null)
            continueChatRequest.ChatRequest = new ChatRequest
            {
                ChatOptions = new ChatOptions
                {
                    PluginsAutoInvoke = false
                }
            };

        var chatRequest = continueChatRequest.ChatRequest;

        if (chatRequest.SearchOptions != null
            && chatRequest.ProvidedDocumentSources.IsNullOrEmpty())
        {
            var retrievedContextSources = await _retrievalHandler.RetrieveContextForQuery(chatRequest);
            if (retrievedContextSources.IsNullOrEmpty())
            {
                return new ChatResponse($"Failed to retrieve contextSources for data retrieval. Ensure search options are correct and database has data.");
            }

            chatRequest.ProvidedDocumentSources = retrievedContextSources;
        }

        var llmService = _llmServiceFactory.Create(chatRequest.ChatOptions);

        var chatResponse = await llmService.ContinueChatResponse(continueChatRequest.PreviousChatHistoryJson, chatRequest.ChatMessages, chatRequest.ProvidedDocumentSources, chatRequest.ChatOptions);

        return chatResponse;
    }
}
