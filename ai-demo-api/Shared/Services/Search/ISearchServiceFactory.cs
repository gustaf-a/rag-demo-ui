using AiDemos.Api.Models;

namespace Shared.Services.Search;

public interface ISearchServiceFactory
{
    ISearchService Create(SearchOptions earchOptions);
    ISearchService Create();
}