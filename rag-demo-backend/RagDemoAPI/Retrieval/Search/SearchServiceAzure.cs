using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;
using RagDemoAPI.Services;

namespace RagDemoAPI.Retrieval.Search;

public class SearchServiceAzure(IConfiguration configuration, Kernel _kernel, IEmbeddingService _embeddingService) : ISearchService
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    public async Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest)
    {
        var chatMessages = chatRequest.ChatMessages;
        var options = chatRequest.ChatRequestOptions;

        var queryEmbeddings = options.UseVectorSearch
                                ? await _embeddingService.GetEmbeddingsAsync(chatMessages.Last().Content)
                                : [];

        var textSearchQuery = options.UseTextSearch
                            ? await GenerateSearchQueryForTextSearch(chatMessages, options)
        : string.Empty;

        var retrievedSources = await RetrieveDocumentsInternal(chatRequest.ChatRequestOptions, queryEmbeddings, textSearchQuery);

        return retrievedSources;
    }

    private async Task<string> GenerateSearchQueryForTextSearch(IEnumerable<ChatMessage> chatMessages, ChatRequestOptions options)
    {
        var searchQueryGenerationChatHistory = CreateSearchQueryGenerationPrompt(chatMessages);

        var chatService = _kernel.GetRequiredService<IChatCompletionService>();

        var searchQueryGenerationResult = await chatService.GetChatMessageContentAsync(searchQueryGenerationChatHistory);

        return searchQueryGenerationResult?.Content
            ?? throw new Exception("Failed to generate search query.");
    }

    private static ChatHistory CreateSearchQueryGenerationPrompt(IEnumerable<ChatMessage> chatMessages)
    {
        var createSearchQueryChatHistory = new ChatHistory(
"""
Your task is to generate search queries used to retrieve data used to answer the users question.
Make the response short, simple and precise.
Do NOT return any text except for the search query.

Example:
employee roles AND employee benefits
employee onboarding AND recipes AND safety instructions
""");

        createSearchQueryChatHistory.AddUserMessage(
$"""
Generate a search query for this query:
{chatMessages.Last().Content}
""");

        return createSearchQueryChatHistory;
    }

    private async Task<IEnumerable<RetrievedDocument>> RetrieveDocumentsInternal(ChatRequestOptions chatRequestOptions, float[]? queryEmbeddings = null, string? textSearchQuery = null)
    {
        var searchOptions = new SearchOptions
        {
            Filter = CreateSearchOptionsFilter(chatRequestOptions.TextSearchRetrievalOptions),
            Size = 3
        };

        var vectorSearchOptions = chatRequestOptions.VectorSearchRetrievalOptions;

        if (vectorSearchOptions.UseSemanticRanker)
        {
            searchOptions.QueryType = SearchQueryType.Semantic;
            searchOptions.SemanticSearch = new()
            {
                SemanticConfigurationName = "default",
                QueryCaption = new(vectorSearchOptions.UseSemanticCaptions
                    ? QueryCaptionType.Extractive
                    : QueryCaptionType.None)
            };
        }

        if (queryEmbeddings is not null)
        {

            var vectorizedQueryEmbeddings = new VectorizedQuery(queryEmbeddings)
            {
                KNearestNeighborsCount = vectorSearchOptions.UseSemanticRanker
                    ? vectorSearchOptions.SemanticRankerCandidatesToRetrieve
                    : vectorSearchOptions.ItemsToRetrieve
            };

            vectorizedQueryEmbeddings.Fields.Add("embedding");

            searchOptions.VectorSearch = new();
            searchOptions.VectorSearch.Queries.Add(vectorizedQueryEmbeddings);
        }

        var searchClient = new SearchClient(new Uri(_azureOptions.SearchService.Endpoint), _azureOptions.SearchService.Name, new Azure.AzureKeyCredential(_azureOptions.SearchService.ApiKey));

        var searchResultResponse = await searchClient.SearchAsync<SearchDocument>(
            textSearchQuery, searchOptions);

        if (searchResultResponse.Value is null)
        {
            throw new Exception("Failed to get search results.");
        }

        var retrievedDocuments = new List<RetrievedDocument>();

        foreach (var searchDocument in searchResultResponse.Value.GetResults())
        {
            RetrievedDocument retrievedDocument = GetRetrievedDocument(searchDocument, chatRequestOptions.VectorSearchRetrievalOptions.UseSemanticCaptions);

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

    private static string CreateSearchOptionsFilter(RetrievalOptions textSearchRetrievalOptions)
    {
        var categoriesToExclude = textSearchRetrievalOptions.CategoryExclude.ToList();
        var categoriesToInclude = textSearchRetrievalOptions.CategoryInclude.ToList();

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
