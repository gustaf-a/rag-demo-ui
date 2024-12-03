using AiDemos.Api.Models;

namespace Shared.Search;

public class SearchServiceFactory(IEnumerable<ISearchService> _searchServices) : ISearchServiceFactory
{
    //TODO see which are online
    public ISearchService Create()
    {
        return _searchServices.Last();
    }

    //TODO take preferred
    public ISearchService Create(SearchOptions searchOptions)
    {
        return Create();
    }
}
