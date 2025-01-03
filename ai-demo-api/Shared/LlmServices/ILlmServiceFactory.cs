using Shared.Models;
using Shared.Models;

namespace Shared.Generation.LlmServices;

public interface ILlmServiceFactory
{
    ILlmService Create(ChatOptions chatRequestOptions);
    ILlmService Create(SearchOptions searchOptions);
    ILlmService Create(IngestDataRequest request);
    ILlmService Create();
}