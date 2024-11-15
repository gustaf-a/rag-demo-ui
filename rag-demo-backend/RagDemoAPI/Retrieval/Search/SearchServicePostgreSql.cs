using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;
using RagDemoAPI.Repositories;
using RagDemoAPI.Services;

namespace RagDemoAPI.Retrieval.Search;

public class SearchServicePostgreSql(ILogger<SearchServicePostgreSql> _logger,
                                     IConfiguration configuration,
                                     IPostgreSqlRepository _postgreSqlService,
                                     IEmbeddingService _embeddingService,
                                     ILlmServiceFactory llmServiceFactory)
            : SearchServiceBase(llmServiceFactory), ISearchService
{
    private readonly PostgreSqlOptions _postgreSqlOptions = configuration.GetSection(PostgreSqlOptions.PostgreSql).Get<PostgreSqlOptions>() ?? throw new ArgumentNullException(nameof(PostgreSqlOptions));

    public async Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest)
    {
        var searchOptions = chatRequest.SearchOptions;

        if (string.IsNullOrWhiteSpace(searchOptions.SemanticSearchContent))
            searchOptions.SemanticSearchContent = await GetQueryContentFromChatMessages(chatRequest.ChatMessages, searchOptions);

        return await RetrieveDocumentsInternal(searchOptions);
    }

    public async Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(SearchRequest searchRequest)
    {
        return await RetrieveDocumentsInternal(searchRequest.SearchOptions);
    }

    private async Task<IEnumerable<RetrievedDocument>> RetrieveDocumentsInternal(SearchOptions searchOptions)
    {
        ArgumentNullException.ThrowIfNull(searchOptions);

        var searchContent = searchOptions.SemanticSearchContent;

        if (string.IsNullOrWhiteSpace(searchContent))
            throw new ArgumentException("Search content cannot be null or whitespace.", nameof(searchContent));

        float[] embeddingQuery = !string.IsNullOrWhiteSpace(searchContent)
                                    ? await _embeddingService.GetEmbeddings(searchContent)
                                    : [];

        var queryParameters = new PostgreSqlQueryParameters
        {
            ContentMustIncludeWords = searchOptions.ContentMustIncludeWords.PostgreSqlEscapeSqlLiteral(),
            ContentMustNotIncludeWords = searchOptions.ContentMustNotIncludeWords.PostgreSqlEscapeSqlLiteral(),
            EmbeddingQuery = embeddingQuery,
            MetaDataFilterInclude = CreateMetaDataFilter(searchOptions.MetaDataFiltersInclude),
            MetaDataFilterExclude = CreateMetaDataFilter(searchOptions.MetaDataFiltersExclude),
            ItemsToRetrieve = searchOptions.ItemsToRetrieve,
            ItemsToSkip = searchOptions.ItemsToSkip,
            SemanticRankerCandidatesToRetrieve = searchOptions.SemanticRankerCandidatesToRetrieve
        };

        try
        {
            var retrievedDocuments = await _postgreSqlService.RetrieveData(searchOptions.DatabaseOptions, queryParameters);

            return retrievedDocuments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents from PostgreSQL. Ensure extensions are installed.");
            throw;
        }
    }

    /// <summary>
    /// Example: metadata @> '{"Category": "A"}'::jsonb
    /// SELECT * FROM users WHERE metadata @> '{"Category": "A"}'; 
    /// </summary>
    private const string PostgreSqlJsonBContainsOperator = "@>";

    private static IEnumerable<string> CreateMetaDataFilter(Dictionary<string, IEnumerable<string>> metaDataFilters)
    {
        if (metaDataFilters.IsNullOrEmpty())
            return [];

        List<string> postgreSqlFilters = [];

        foreach (var filter in metaDataFilters)
        {
            if (filter.Value.IsNullOrEmpty())
            {
                throw new ArgumentException($"MetaDataFilter {filter.Key} needs to have values.");
            }

            string key = filter.Key.PostgreSqlEscapeSqlIdentifier();

            foreach (var filterValue in filter.Value)
            {
                if (string.IsNullOrWhiteSpace(filterValue))
                    continue;

                string value = filterValue.PostgreSqlEscapeSqlLiteral();

                string postgreSqlFilter = $"metadata {PostgreSqlJsonBContainsOperator} '{{\"{key}\": \"{value}\"}}'";
                postgreSqlFilters.Add(postgreSqlFilter);
            }
        }

        return postgreSqlFilters;
    }
}
