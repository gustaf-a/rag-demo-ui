using AiDemos.Api.Models;

namespace Shared.Search;

public interface ISearchServiceFactory
{
    ISearchService Create(SearchOptions earchOptions);
    ISearchService Create();
}