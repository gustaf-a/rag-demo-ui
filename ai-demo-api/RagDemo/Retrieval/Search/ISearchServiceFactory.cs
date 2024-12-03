using AiDemos.Api.Models;
using AiDemos.Api.Retrieval.Search;

namespace AiDemos.Api.Generation.LlmServices;

public interface ISearchServiceFactory
{
    ISearchService Create(SearchOptions earchOptions);
    ISearchService Create();
}