using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;
using RagDemoAPI.Plugins;

namespace RagDemoAPI.Generation.LlmServices;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class LlmServiceSemanticKernel(IConfiguration configuration, Kernel _kernel, IPluginHandler _pluginHandler) : ILlmService
{
    public async Task<ChatResponse> GetChatResponse(IEnumerable<Models.ChatMessage> chatMessages, ChatOptions chatOptions)
    {
        return await GetChatResponseInternal(chatMessages.ToSemanticKernelChatMessages(), chatOptions);
    }

    public async Task<ChatResponse> GetChatResponse(IEnumerable<Models.ChatMessage> chatMessages, IEnumerable<RetrievedDocument> retrievedContextSources, ChatOptions chatOptions)
    {
        var chatResponse = await GetChatResponseInternal(chatMessages.ToSemanticKernelChatMessages(retrievedContextSources), chatOptions);
        
        chatResponse.Citations = retrievedContextSources.ToList();

        return chatResponse;
    }

    private async Task<ChatResponse> GetChatResponseInternal(ChatHistory chatHistory, ChatOptions chatOptions)
    {
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            Temperature = chatOptions.Temperature
        };

        if (!chatOptions.PluginsToUse.IsNullOrEmpty())
        {
            _pluginHandler.AddPlugins(_kernel, chatOptions.PluginsToUse);

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
            ChatHistory = chatHistory
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
