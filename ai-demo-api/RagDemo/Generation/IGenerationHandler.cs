using AiDemos.Api.Models;

namespace AiDemos.Api.Generation
{
    public interface IGenerationHandler
    {
        Task<ChatResponse> ContinueChatResponse(ContinueChatRequest continueChatRequest);
        Task<ChatResponse> GetChatResponse(ChatRequest chatRequest);
    }
}