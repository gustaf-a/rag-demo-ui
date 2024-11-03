using RagDemoAPI.Models;

namespace RagDemoAPI.Generation.LlmServices;

public interface ILlmServiceFactory
{
    ILlmService Create(ChatRequestOptions chatRequestOptions);
    ILlmService Create(IngestDataRequest request);
    ILlmService Create();
}