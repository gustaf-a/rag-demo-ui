using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;

namespace RagDemoAPI.Retrieval;

public class RetrievalHandler(ISearchServiceFactory _searchServiceFactory) : IRetrievalHandler
{
    //TODO Behövs inte. Hur text-search används är upp till searchService
    //Här ska vi bestämma vilken searchservice som tillkallas.
    public async Task<IEnumerable<RetrievedDocument>> RetrieveContextForQuery(ChatRequest chatRequest)
    {
        if (chatRequest is null || chatRequest.SearchOptions is null)
            throw new ArgumentNullException(nameof(ChatRequest.SearchOptions));

        var searchOptions = chatRequest.SearchOptions;

        var searchService = _searchServiceFactory.Create(searchOptions);

        var retrievedSources = await searchService.RetrieveDocuments(chatRequest);

        return retrievedSources;
    }
}
