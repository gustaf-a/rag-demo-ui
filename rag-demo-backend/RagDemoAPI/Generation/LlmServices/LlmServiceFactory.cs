using RagDemoAPI.Models;

namespace RagDemoAPI.Generation.LlmServices;

public class LlmServiceFactory(IEnumerable<ILlmService> _llmServices) : ILlmServiceFactory
{
    public ILlmService Create(ChatRequestOptions chatRequestOptions)
    {
        return Create();
    }

    public ILlmService Create(IngestDataRequest request)
    {
        return Create();
    }

    public ILlmService Create()
    {
        return _llmServices.First();
    }
}
