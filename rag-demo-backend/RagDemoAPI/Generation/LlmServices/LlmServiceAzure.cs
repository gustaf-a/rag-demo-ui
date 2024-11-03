using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using OpenAI.Chat;
using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;

namespace RagDemoAPI.Generation.LlmServices;

#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class LlmServiceAzure(IConfiguration configuration) : ILlmService
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    public async Task<ChatResponse> GetChatResponse(IEnumerable<Models.ChatMessage> chatMessages)
    {
        return await GetChatResponse(chatMessages, [], new ChatOptions());
    }

    public async Task<ChatResponse> GetChatResponse(IEnumerable<Models.ChatMessage> chatMessages, IEnumerable<RetrievedDocument> retrievedContextSources, ChatOptions chatRequestOptions)
    {
        var chatHistory = chatMessages.ToOpenAiChatMessages(retrievedContextSources);

        var chatClient = GetAzureChatClient();

        ChatCompletionOptions options = new();

        var clientResult = await chatClient.CompleteChatAsync(
            chatHistory,
            options);

        var chatresult = clientResult.Value;

        var chatResultContext = chatresult.GetMessageContext();
        if (chatResultContext is null)
        {
            return new ChatResponse(chatresult.Content[0].Text);
        }

        return new ChatResponse(chatresult.Content[0].Text, intent: chatResultContext.Intent, citations: chatResultContext.Citations);
    }

    public async Task<string> GetCompletionSimple(string prompt)
    {
        var chatHistory = new List<OpenAI.Chat.ChatMessage>
        {
            new UserChatMessage(prompt)
        };

        var chatClient = GetAzureChatClient();

        ChatCompletionOptions options = new();

        var clientResult = await chatClient.CompleteChatAsync(
            chatHistory,
            options);

        var chatresult = clientResult.Value;

        return chatresult.Content[0].Text;
    }

    private ChatClient GetAzureChatClient()
    {
        AzureOpenAIClient azureClient = new(
        new Uri(_azureOptions.ChatService.Endpoint),
                    new System.ClientModel.ApiKeyCredential(_azureOptions.ChatService.ApiKey)
        );

        var chatClient = azureClient.GetChatClient(_azureOptions.ChatService.Name);
        return chatClient;
    }
}
