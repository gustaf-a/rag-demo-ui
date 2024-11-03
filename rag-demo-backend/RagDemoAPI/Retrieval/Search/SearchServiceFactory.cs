using RagDemoAPI.Models;
using RagDemoAPI.Retrieval.Search;

namespace RagDemoAPI.Generation.LlmServices;

public class SearchServiceFactory(IEnumerable<ISearchService> _searchServices) : ISearchServiceFactory
{
    public ISearchService Create()
    {
        return _searchServices.First();
    }

    public ISearchService Create(SearchOptions searchOptions)
    {
        return Create();
    }
}
