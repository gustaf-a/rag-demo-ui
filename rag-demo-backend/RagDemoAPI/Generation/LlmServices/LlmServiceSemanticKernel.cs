using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;
using RagDemoAPI.Plugins;

namespace RagDemoAPI.Generation.LlmServices;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class LlmServiceSemanticKernel(IConfiguration configuration, Kernel _kernel, IPluginHandler _pluginHandler) : ILlmService
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    public async Task<ChatResponse> GetChatResponse(IEnumerable<Models.ChatMessage> chatMessages, ChatOptions chatRequestOptions)
    {
        return await GetChatResponse(chatMessages, [], chatRequestOptions);
    }

    public async Task<ChatResponse> GetChatResponse(IEnumerable<Models.ChatMessage> chatMessages, IEnumerable<RetrievedDocument> retrievedContextSources, ChatOptions chatRequestOptions)
    {
        var chatHistory = chatMessages.ToSemanticKernelChatMessages(retrievedContextSources);

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            Temperature = chatRequestOptions.Temperature
        };

        if (!chatRequestOptions.PluginsToUse.IsNullOrEmpty())
        {
            _pluginHandler.AddPlugins(_kernel, chatRequestOptions.PluginsToUse);

            openAIPromptExecutionSettings.FunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
        }

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var result = await chatCompletionService.GetChatMessageContentAsync(
           chatHistory,
           executionSettings: openAIPromptExecutionSettings,
           kernel: _kernel) 
            ?? throw new Exception($"Failed to get chat completion response.");
        
        return new ChatResponse(result.Content)
        {
            ChatHistory = chatHistory,
            Citations = retrievedContextSources.ToList()
        };
    }

    public async Task<string> GetCompletionSimple(string prompt)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
        };

        var result = await chatCompletionService.GetChatMessageContentAsync(
           prompt,
           executionSettings: openAIPromptExecutionSettings,
           kernel: _kernel);

        var chatresult = result?.InnerContent?.ToString() ?? string.Empty;

        return chatresult;
    }
}
