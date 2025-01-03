using Shared.Models;
using Shared.Models;

namespace Shared.Generation.LlmServices
{
    public interface ILlmService
    {
        Task<ChatResponse> GetChatResponse(IEnumerable<ChatMessage> chatMessages, ChatOptions chatRequestOptions);
        Task<ChatResponse> GetChatResponse(IEnumerable<ChatMessage> chatMessages, IEnumerable<RetrievedDocument> retrievedContextSources, ChatOptions chatRequestOptions);
        Task<ChatResponse> ContinueChatResponse(string previousChatHistoryJson, IEnumerable<ChatMessage> chatMessages, IEnumerable<RetrievedDocument> retrievedContextSources, ChatOptions chatRequestOptions);
        Task<string> GetCompletionSimple(string contextEnrichedChunkPrompt);
    }
}