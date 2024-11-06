using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;
using RagDemoAPI.Services;

namespace RagDemoAPI.Retrieval.Search;

public class SearchServicePostgreSql(ILogger<SearchServicePostgreSql> _logger, IConfiguration configuration, IPostgreSqlService _postgreSqlService, IEmbeddingService _embeddingService) : ISearchService
{
    private readonly PostgreSqlOptions _postgreSqlOptions = configuration.GetSection(PostgreSqlOptions.PostgreSql).Get<PostgreSqlOptions>() ?? throw new ArgumentNullException(nameof(PostgreSqlOptions));

    public async Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest)
    {
        var chatMessages = chatRequest.ChatMessages;
        var searchOptions = chatRequest.SearchOptions;

        var searchContent = GetQueryContentFromChatMessage(chatMessages, searchOptions);

        return await RetrieveDocumentsInternal(searchOptions, searchContent);
    }

    //TODO Move to base class
    private string GetQueryContentFromChatMessage(IEnumerable<ChatMessage> chatMessages, SearchOptions searchOptions)
    {
        if (chatMessages.IsNullOrEmpty())
            return string.Empty;

        if (searchOptions.SemanticSearchGenerateSummaryOfNMessages > 0)
        {
            //TODO generate a summary of all messages.
            throw new NotImplementedException();
        }
        else
        {
            return chatMessages.Last().Content;
        }
    }

    public async Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(SearchRequest searchRequest)
    {
        return await RetrieveDocumentsInternal(searchRequest.SearchOptions, searchRequest.SearchOptions.SemanticSearchContent);
    }

    private async Task<IEnumerable<RetrievedDocument>> RetrieveDocumentsInternal(SearchOptions searchOptions, string searchContent)
    {
        ArgumentNullException.ThrowIfNull(searchOptions);

        if (string.IsNullOrWhiteSpace(searchContent))
            throw new ArgumentException("Search content cannot be null or whitespace.", nameof(searchContent));

        float[] embeddingQuery = !string.IsNullOrWhiteSpace(searchContent)
                                    ? await _embeddingService.GetEmbeddings(searchContent)
                                    : [];

        var queryParameters = new PostgreSqlQueryParameters
        {
            TextQuery = searchOptions.TextSearchContent,
            MetaDataSearchQuery = searchOptions.TextSearchMetaData,
            EmbeddingQuery = embeddingQuery,
            MetaDataFilterInclude = [],
            MetaDataFilterExclude = [],
            ItemsToRetrieve = searchOptions.ItemsToRetrieve,
            ItemsToSkip = searchOptions.ItemsToSkip,
            UseSemanticCaptions = searchOptions.UseSemanticCaptions,
            SemanticRankerCandidatesToRetrieve = searchOptions.SemanticRankerCandidatesToRetrieve
        };

        try
        {
            var retrievedDocuments = await _postgreSqlService.RetrieveData(queryParameters);

            return retrievedDocuments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents from PostgreSQL.");
            throw;
        }
    }

    //private static List<string> CreateMetaDataIncludeFilters(SearchOptions searchOptions)
    //{


    //    var filters = new List<string>();

    //    if (searchOptions.MetaDataFiltersInclude != null && searchOptions.MetaDataFiltersInclude.Any())
    //    {
    //        filters.Add($"category IN ({string.Join(", ", searchOptions.MetaDataFiltersInclude.Select(c => $"'{c}'"))})");
    //    }

    //    if (searchOptions.CategoriesExclude != null && searchOptions.CategoriesExclude.Any())
    //    {
    //        filters.Add($"category NOT IN ({string.Join(", ", searchOptions.CategoriesExclude.Select(c => $"'{c}'"))})");
    //    }

    //    return filters;
    //}

    //private static List<string> CreateMetaDataExcludeFilters(SearchOptions searchOptions)
    //{
    //    var filters = new List<string>();

    //    if (searchOptions.MetaDataFiltersInclude != null && searchOptions.MetaDataFiltersInclude.Any())
    //    {
    //        filters.Add($"category IN ({string.Join(", ", searchOptions.MetaDataFiltersInclude.Select(c => $"'{c}'"))})");
    //    }

    //    if (searchOptions.CategoriesExclude != null && searchOptions.CategoriesExclude.Any())
    //    {
    //        filters.Add($"category NOT IN ({string.Join(", ", searchOptions.CategoriesExclude.Select(c => $"'{c}'"))})");
    //    }

    //    return filters;
    //}
}
