using RagDemoAPI.Models;

namespace RagDemoAPI.Generation.LlmServices;

public interface ILlmServiceFactory
{
    ILlmService Create(ChatOptions chatRequestOptions);
    ILlmService Create(SearchOptions searchOptions);
    ILlmService Create(IngestDataRequest request);
    ILlmService Create();
}