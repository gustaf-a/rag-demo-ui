﻿using RagDemoAPI.Models;

namespace RagDemoAPI.Generation.LlmServices
{
    public interface ILlmService
    {
        Task<ChatResponse> GetChatResponse(IEnumerable<ChatMessage> chatMessages);
        Task<ChatResponse> GetChatResponse(IEnumerable<ChatMessage> chatMessages, IEnumerable<RetrievedDocument> retrievedContextSources, ChatRequestOptions chatRequestOptions);
        Task<string> GetCompletionSimple(string contextEnrichedChunkPrompt);
    }
}