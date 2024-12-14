using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using AiDemos.Api.Configuration;
using AiDemos.Api.Extensions;
using AiDemos.Api.Generation.LlmServices;
using AiDemos.Api.Models;
using AiDemos.Api.Services;
using SearchOptions = AiDemos.Api.Models.SearchOptions;

namespace Shared.Services.Search;

public class SearchServiceAzure(IConfiguration configuration,
                                ILlmServiceFactory _llmServiceFactory,
                                IEmbeddingService _embeddingService)
    : SearchServiceBase(_llmServiceFactory), ISearchService
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    public async Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest)
    {
        var chatMessages = chatRequest.ChatMessages;
        var searchOptions = chatRequest.SearchOptions;

        var searchContent = await GetQueryContentFromChatMessages(chatMessages, searchOptions);

        return await RetrieveDocumentsInternal(searchOptions, searchContent);
    }

    public async Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(SearchRequest searchRequest)
    {
        return await RetrieveDocumentsInternal(searchRequest.SearchOptions, searchRequest.SearchOptions.SemanticSearchContent);
    }

    private async Task<IEnumerable<RetrievedDocument>> RetrieveDocumentsInternal(SearchOptions searchOptions, string searchContent)
    {
        var queryEmbeddings = await GenerateQueryEmbeddingsInternal(searchContent);

        var textSearchQuery = await GenerateSearchQueryForTextSearch(searchContent, searchOptions);

        var retrievedSources = await RetrieveDocumentsInternal(searchOptions, queryEmbeddings, textSearchQuery);

        return retrievedSources;
    }

    private async Task<float[]> GenerateQueryEmbeddingsInternal(string semanticSearchContent)
    {
        return !string.IsNullOrWhiteSpace(semanticSearchContent)
                                        ? await _embeddingService.GetEmbeddings(semanticSearchContent)
                                        : [];
    }

    private async Task<string> GenerateSearchQueryForTextSearch(string queryContent, SearchOptions searchOptions)
    {
        if (!searchOptions.ContentMustIncludeWords.IsNullOrEmpty()
            || !searchOptions.ContentMustNotIncludeWords.IsNullOrEmpty())
        {
            throw new NotSupportedException();
        }

        var llmService = _llmServiceFactory.Create(searchOptions);

        var searchQueryGenerationResult = await llmService.GetCompletionSimple(GetCreateSearchQueryGenerationPrompt(queryContent));

        return searchQueryGenerationResult
            ?? throw new Exception("Failed to generate search query.");
    }

    private static string GetCreateSearchQueryGenerationPrompt(string queryContent)
    {
        var createSearchQueryPrompt =
$"""
Your task is to generate search queries used to retrieve data used to answer the users question.
Make the response short, simple and precise.
Do NOT return any text except for the search query.

Example:
employee roles AND employee benefits
employee onboarding AND recipes AND safety instructions

Generate a search query for this content:
{queryContent}
""";
        return createSearchQueryPrompt;
    }

    private async Task<IEnumerable<RetrievedDocument>> RetrieveDocumentsInternal(SearchOptions searchOptions, float[]? queryEmbeddings = null, string? textSearchQuery = null)
    {
        var azureSearchOptions = new Azure.Search.Documents.SearchOptions
        {
            Filter = CreateMetaDataFilters(searchOptions),
            Size = 3
        };

        if (searchOptions.UseSemanticReRanker)
        {
            azureSearchOptions.QueryType = SearchQueryType.Semantic;
            azureSearchOptions.SemanticSearch = new()
            {
                SemanticConfigurationName = "default",
                QueryCaption = new(searchOptions.UseSemanticCaptions
                    ? QueryCaptionType.Extractive
                    : QueryCaptionType.None)
            };
        }

        if (queryEmbeddings is not null)
        {
            var vectorizedQueryEmbeddings = new VectorizedQuery(queryEmbeddings)
            {
                KNearestNeighborsCount = searchOptions.UseSemanticReRanker
                    ? searchOptions.SemanticRankerCandidatesToRetrieve
                    : searchOptions.ItemsToRetrieve
            };

            vectorizedQueryEmbeddings.Fields.Add("embedding");

            azureSearchOptions.VectorSearch = new();
            azureSearchOptions.VectorSearch.Queries.Add(vectorizedQueryEmbeddings);
        }

        var searchClient = new SearchClient(new Uri(_azureOptions.SearchService.Endpoint), _azureOptions.SearchService.Name, new Azure.AzureKeyCredential(_azureOptions.SearchService.ApiKey));

        var searchResultResponse = await searchClient.SearchAsync<SearchDocument>(
            textSearchQuery, azureSearchOptions);

        if (searchResultResponse.Value is null)
        {
            throw new Exception("Failed to get search results.");
        }

        var retrievedDocuments = new List<RetrievedDocument>();

        foreach (var searchDocument in searchResultResponse.Value.GetResults())
        {
            RetrievedDocument retrievedDocument = GetRetrievedDocument(searchDocument, searchOptions.UseSemanticCaptions);

            if (retrievedDocument != null)
                retrievedDocuments.Add(retrievedDocument);
        }

        return retrievedDocuments;
    }


    private RetrievedDocument GetRetrievedDocument(SearchResult<SearchDocument> searchDocument, bool useSemanticCaptions)
    {
        searchDocument.Document.TryGetValue("sourcepage", out var sourcePageValue);

        if (sourcePageValue is not string sourcePage)
            return null;

        List<string> contentValues = [];

        try
        {
            if (useSemanticCaptions)
            {
                contentValues.AddRange(searchDocument.SemanticSearch.Captions.Select(c => c.Text).ToList());
            }
            else
            {
                searchDocument.Document.TryGetValue("content", out var value);
                contentValues.Add((string)value);
            }
        }
        catch (ArgumentNullException)
        {
            contentValues.Clear();
        }

        if (!contentValues.Any())
            throw new Exception($"Failed to collect contentValues from response. Semantic Captions: {useSemanticCaptions.ToTrueFalse()}");

        var content = string.Join(" . ", contentValues).RemoveNewLines();

        return new RetrievedDocument(sourcePage, content);
    }

    private static string CreateMetaDataFilters(SearchOptions searchOptions)
    {
        if (searchOptions.MetaDataExclude.IsNullOrEmpty()
            && searchOptions.MetaDataInclude.IsNullOrEmpty())
            return string.Empty;

        var filterParts = new List<string>();

        if (!searchOptions.MetaDataExclude.IsNullOrEmpty())
        {
            foreach (var filterKeyValuePair in searchOptions.MetaDataExclude)
            {
                switch (filterKeyValuePair.Key.ToLower())
                {
                    case "category":
                        filterParts.Add(CreateMetaDataFilterString("category", include: true));
                        break;
                    default:
                        throw new NotSupportedException($"Meta data filter for {filterKeyValuePair.Key} not supported.");

                }

                //var excludeFilters = searchOptions.MetaDataFiltersExclude
                //    .Select(category => $"category ne '{category.EscapeODataValue()}'");
                //var excludeFilter = string.Join(" and ", excludeFilters);
                //filterParts.Add($"({excludeFilter})");
            }
        }

        if (!searchOptions.MetaDataInclude.IsNullOrEmpty())
        {
            foreach (var filterKeyValuePair in searchOptions.MetaDataExclude)
            {
                switch (filterKeyValuePair.Key.ToLower())
                {
                    case "category":
                        filterParts.Add(CreateMetaDataFilterString("category", include: false));
                        break;
                    default:
                        throw new NotSupportedException($"Meta data filter for {filterKeyValuePair.Key} not supported.");

                }

                //    var includeFilters = searchOptions.MetaDataFiltersInclude
                //    .Select(category => $"category eq '{category.EscapeODataValue()}'");
                //var includeFilter = string.Join(" or ", includeFilters);
                //filterParts.Add($"({includeFilter})");
            }
        }

        var finalFilter = string.Join(" and ", filterParts);

        return finalFilter;
    }

    private static string CreateMetaDataFilterString(string metaDataPropertyName, bool include)
    {
        return $"";
    }
}
