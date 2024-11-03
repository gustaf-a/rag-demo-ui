using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;
using RagDemoAPI.Retrieval.Search;
using RagDemoAPI.Services;
using ChatMessage = RagDemoAPI.Models.ChatMessage;

namespace RagDemoAPI.Controllers.EarlyTest;

#pragma warning disable SKEXP0010
[ApiController]
[Route("SemanticKernel")]
public class SemanticKernelController(ILogger<SemanticKernelController> _logger, IConfiguration configuration, Kernel _kernel, IEmbeddingService _embeddingService, ISearchService _searchService) : ControllerBase
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    [HttpPost("chatCompletion")]
    public async Task<string> Get([FromBody] IEnumerable<ChatMessage> chatMessages)
    {
        var prompt = new ChatPrompt(chatMessages);
        var promptString = prompt.ToFormattedString();

        if (string.IsNullOrWhiteSpace(promptString))
        {
            return "No prompt received.";
        }

        var reply = await _kernel.InvokePromptAsync(promptString);

        return reply?.ToString() ?? "No reply";
    }

    /// <summary>
    /// Uses Azure client with data source sent in as an option.
    /// Datasource is used, but citations are not included in response. In the text the citations used are referenced like this '[doc1]', meaning the first item (0 in collection) was used as reference, but the actual documents we cannot access.
    /// Example:
    /// "**Chocolate Dreamweaver**: In charge of turning wild chocolate dreams into reality, requiring creative thinking [doc1]"
    /// </summary>
    [HttpPost("chatCompletion/rag/simple")]
    public async Task<string> CompleteChatRagSimple([FromBody] IEnumerable<ChatMessage> chatMessages)
    {
        var chatHistory = chatMessages.ToSemanticKernelChatMessages();

        var azureSearchDataSource = AzureHelpers.CreateAzureSearchChatDataSource(_azureOptions);
        var azureOpenAIPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings { AzureChatDataSource = azureSearchDataSource };

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory: chatHistory, azureOpenAIPromptExecutionSettings);

        //TODO Get citations from context. Currently not available in semantic kernel 2024-10-20?
        //TODO Maybe through inner content deserialization

        return reply?.ToString() ?? "No reply";
    }

    [HttpPost("chatCompletion/rag/manual")]
    public async Task<ChatResponse> CompleteChatRagManual([FromBody] ChatRequest chatRequest)
    {
        var chatMessages = chatRequest.ChatMessages;

        var retrievedContextSources = await RetrieveContextForQuery(chatRequest, chatMessages);
        if (retrievedContextSources.IsNullOrEmpty())
        {
            return new ChatResponse($"Failed to retrieve contextSources for data retrieval. Ensure at least on of {nameof(ChatRequestOptions.UseTextSearch)} or {nameof(ChatRequestOptions.UseVectorSearch)} is enabled.");
        }

        var chatHistory = chatMessages.ToSemanticKernelChatMessages(retrievedContextSources);

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var azureOpenAIPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = chatRequest.ChatRequestOptions.Temperature
        };

        var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory: chatHistory, azureOpenAIPromptExecutionSettings);

        //TODO Get citations from context. Currently not available in semantic kernel 2024-10-20?
        //TODO Maybe through inner content deserialization

        return new ChatResponse(reply?.ToString() ?? "No reply");
    }

    [HttpPost("semanticSearch/vectorDb")]
    public async Task<SearchResponse> SemanticSearchVectorDatabase([FromBody] SearchRequest searchRequest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchRequest.SearchQuery);

        float[]? queryEmbeddings = await _embeddingService.GetEmbeddingsAsync(searchRequest.SearchQuery);

        var retrievedSources = await _searchService.RetrieveDocuments(searchRequest.VectorSearchRetrievalOptions, queryEmbeddings);

        if (retrievedSources.IsNullOrEmpty())
        {
            return new SearchResponse("No sources found.");
        }

        return new SearchResponse(retrievedSources);
    }

    private async Task<IEnumerable<RetrievedDocument>> RetrieveContextForQuery(ChatRequest chatRequest, IEnumerable<ChatMessage> chatMessages)
    {
        var options = chatRequest.ChatRequestOptions;

        float[]? queryEmbeddings = options.UseVectorSearch
                                ? await _embeddingService.GetEmbeddingsAsync(chatMessages.Last().Content)
                                : [];

        var textSearchQuery = options.UseTextSearch
                            ? await _searchService.GenerateSearchQueryForTextSearch(chatMessages, options)
                            : string.Empty;

        var retrievedSources = await _searchService.RetrieveDocuments(chatRequest.ChatRequestOptions, queryEmbeddings, textSearchQuery);

        return retrievedSources;
    }



    //TODO skapa manuellt hämta knowledge först, och om är tillräckligt relevant, då skicka för chat completions

    //TODO Semantic search with paging
}
