using Shared.Extensions;
using Shared.Models;
using Microsoft.SemanticKernel;
using Shared.Services.Search;
using System.ComponentModel;

namespace Shared.Plugins;

public class SearchDatabasePlugin : IPlugin
{
    private readonly ISearchServiceFactory _searchServiceFactory;

    public SearchDatabasePlugin(ISearchServiceFactory searchServiceFactory)
    {
        _searchServiceFactory = searchServiceFactory;
    }

    [KernelFunction("get_sources")]
    [Description("Returns 3 most relevant sources from database using semantic search.")]
    public async Task<string> GetMostRelevantSources([Description("Natural language for the semantic search. A question, statement, intent, or keywords.")] string searchPhrase)
    {
        if (string.IsNullOrWhiteSpace(searchPhrase))
        {
            return "Invalid searchPhrase. No input provided.";
        }

        var searchOptions = new SearchOptions
        {
            EmbeddingsTableName = "embeddings1",
            ItemsToRetrieve = 3,
            SemanticSearchContent = searchPhrase
        };

        var searchRequest = new SearchRequest
        {
            SearchOptions = searchOptions
        };

        var searchService = _searchServiceFactory.Create(searchOptions);

        var retrievedSources = await searchService.RetrieveDocuments(searchRequest);

        var sourcesString = retrievedSources.ToSourcesString();

        return sourcesString;
    }
}
