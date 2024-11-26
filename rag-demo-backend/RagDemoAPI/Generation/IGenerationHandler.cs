using RagDemoAPI.Models;

namespace RagDemoAPI.Generation
{
    public interface IGenerationHandler
    {
        Task<ChatResponse> ContinueChatResponse(ContinueChatRequest continueChatRequest);
        Task<ChatResponse> GetChatResponse(ChatRequest chatRequest);
    }
}