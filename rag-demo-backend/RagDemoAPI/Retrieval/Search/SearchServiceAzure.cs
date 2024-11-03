using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Generation.LlmServices;
using RagDemoAPI.Models;
using RagDemoAPI.Services;
using SearchOptions = RagDemoAPI.Models.SearchOptions;

namespace RagDemoAPI.Retrieval.Search;

public class SearchServiceAzure(IConfiguration configuration, ILlmServiceFactory _llmServiceFactory, IEmbeddingService _embeddingService) : ISearchService
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));
    
    public async Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest)
    {
        var chatMessages = chatRequest.ChatMessages;
        var searchOptions = chatRequest.SearchOptions;

        var searchContent = GetQueryContentForSearch(chatMessages, searchOptions);

        return await RetrieveDocumentsInternal(searchOptions, searchContent);
    }

    private string GetQueryContentForSearch(IEnumerable<ChatMessage> chatMessages, SearchOptions searchOptions)
    {
        if (chatMessages.IsNullOrEmpty())
            return string.Empty;

        if (searchOptions.SemanticSearchGenerateSummaryOfMessageHistory)
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
        return await RetrieveDocumentsInternal(searchRequest.SearchOptions, searchRequest.SearchOptions.SearchContent);
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
                                        ? await _embeddingService.GetEmbeddingsAsync(semanticSearchContent)
                                        : [];
    }
    
    private async Task<string> GenerateSearchQueryForTextSearch(string queryContent, SearchOptions searchOptions)
    {
        if(string.IsNullOrWhiteSpace(searchOptions.TextSearchContent)
            && string.IsNullOrWhiteSpace(searchOptions.TextSearchMetaData))
            return string.Empty;

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
            Filter = CreateSearchOptionsFilter(searchOptions),
            Size = 3
        };

        if (searchOptions.UseSemanticRanker)
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
                KNearestNeighborsCount = searchOptions.UseSemanticRanker
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

    private static string CreateSearchOptionsFilter(SearchOptions searchOptions)
    {
        var categoriesToExclude = searchOptions.CategoriesExclude.ToList();
        var categoriesToInclude = searchOptions.CategoriesInclude.ToList();

        if (categoriesToExclude.IsNullOrEmpty() && categoriesToInclude.IsNullOrEmpty())
            return string.Empty;

        var filterParts = new List<string>();

        if (!categoriesToExclude.IsNullOrEmpty())
        {
            var excludeFilters = categoriesToExclude
                .Select(category => $"category ne '{category.EscapeODataValue()}'");
            var excludeFilter = string.Join(" and ", excludeFilters);
            filterParts.Add($"({excludeFilter})");
        }

        if (!categoriesToInclude.IsNullOrEmpty())
        {
            var includeFilters = categoriesToInclude
                .Select(category => $"category eq '{category.EscapeODataValue()}'");
            var includeFilter = string.Join(" or ", includeFilters);
            filterParts.Add($"({includeFilter})");
        }

        var finalFilter = string.Join(" and ", filterParts);

        return finalFilter;
    }
}
