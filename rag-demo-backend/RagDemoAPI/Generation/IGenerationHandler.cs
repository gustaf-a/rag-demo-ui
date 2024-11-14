using RagDemoAPI.Models;

namespace RagDemoAPI.Generation
{
    public interface IGenerationHandler
    {
        Task<ChatResponse> GetChatResponse(ChatRequest chatRequest);
        Task<ChatResponse> GetRetrievalAugmentedChatResponse(ChatRequest chatRequest);
    }
}