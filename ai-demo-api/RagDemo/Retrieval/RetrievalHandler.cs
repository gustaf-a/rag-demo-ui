using AiDemos.Api.Models;
using Shared.Services.Search;

namespace AiDemos.Api.Retrieval;

public class RetrievalHandler(ISearchServiceFactory _searchServiceFactory) : IRetrievalHandler
{
    public async Task<IEnumerable<RetrievedDocument>> DoSearch(SearchRequest searchRequest)
    {
        ArgumentNullException.ThrowIfNull(searchRequest);
        ArgumentNullException.ThrowIfNull(searchRequest.SearchOptions);

        var searchService = _searchServiceFactory.Create(searchRequest.SearchOptions);

        var retrievedSources = await searchService.RetrieveDocuments(searchRequest);

        return retrievedSources;
    }

    public async Task<IEnumerable<RetrievedDocument>> RetrieveContextForQuery(ChatRequest chatRequest)
    {
        ArgumentNullException.ThrowIfNull(chatRequest);
        ArgumentNullException.ThrowIfNull(chatRequest.SearchOptions);

        var searchService = _searchServiceFactory.Create(chatRequest.SearchOptions);

        var retrievedSources = await searchService.RetrieveDocuments(chatRequest);

        return retrievedSources;
    }
}
