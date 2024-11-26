using RagDemoAPI.Extensions;
using RagDemoAPI.Models;

namespace RagDemoAPI.Generation.LlmServices;

public class LlmServiceFactory(IEnumerable<ILlmService> _llmServices) : ILlmServiceFactory
{
    public ILlmService Create(ChatOptions chatRequestOptions)
    {
        if (chatRequestOptions is null)
            return Create();

        if (!chatRequestOptions.PluginsToUse.IsNullOrEmpty())
            return CreateWithPlugins(chatRequestOptions);

        if(!chatRequestOptions.PluginsAutoInvoke ?? false)
            return CreateWithPlugins(chatRequestOptions);

        return Create();
    }

    private ILlmService CreateWithPlugins(ChatOptions chatRequestOptions)
    {
        return _llmServices.Where(llms => llms.GetType().Name == nameof(LlmServiceSemanticKernel)).FirstOrDefault()
            ?? throw new Exception("Failed to resolve LlmService for plugin use.");
    }

    public ILlmService Create(IngestDataRequest request)
    {
        return Create();
    }

    public ILlmService Create(SearchOptions searchOptions)
    {
        return Create();
    }

    public ILlmService Create()
    {
        return _llmServices.First();
    }
}
