using RagDemoAPI.Models;

namespace RagDemoAPI.Generation
{
    public interface IGenerationHandler
    {
        Task<ChatResponse> GetChatResponse(IEnumerable<ChatMessage> chatMessages);
        Task<ChatResponse> GetRetrievalAugmentedChatResponse(ChatRequest chatRequest);
    }
}