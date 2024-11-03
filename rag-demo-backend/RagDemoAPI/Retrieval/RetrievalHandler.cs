using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;

namespace RagDemoAPI.Retrieval;

public class RetrievalHandler(ISearchServiceFactory _searchServiceFactory) : IRetrievalHandler
{
    //TODO Behövs inte. Hur text-search används är upp till searchService
    //Här ska vi bestämma vilken searchservice som tillkallas.
    public async Task<IEnumerable<RetrievedDocument>> RetrieveContextForQuery(ChatRequest chatRequest)
    {
        if (chatRequest is null || chatRequest.ChatRequestOptions is null)
            throw new ArgumentNullException(nameof(ChatRequest.ChatRequestOptions));

        var chatRequestOptions = chatRequest.ChatRequestOptions;

        if (!chatRequestOptions.UseVectorSearch && !chatRequestOptions.UseTextSearch)
            throw new ArgumentException($"Both {nameof(ChatRequestOptions.UseTextSearch)} and {nameof(ChatRequestOptions.UseVectorSearch)} can't be false.");

        var searchService = _searchServiceFactory.Create(chatRequestOptions);

        var retrievedSources = await searchService.RetrieveDocuments(chatRequest);

        return retrievedSources;
    }
}
