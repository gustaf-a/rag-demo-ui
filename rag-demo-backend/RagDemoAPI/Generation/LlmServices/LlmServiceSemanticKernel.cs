using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;
using RagDemoAPI.Plugins;
using System.Text.Json;
using ChatOptions = RagDemoAPI.Models.ChatOptions;

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

    public async Task<ChatResponse> ContinueChatResponse(string previousChatHistoryJson, IEnumerable<Models.ChatMessage> chatMessages, IEnumerable<Models.RetrievedDocument> retrievedContextSources, ChatOptions chatRequestOptions)
    {
        if(string.IsNullOrWhiteSpace(previousChatHistoryJson))
            throw new Exception($"Cannot continue chat if previous chat history is nothing.");

        var chatHistory = JsonSerializer.Deserialize<ChatHistory>(previousChatHistoryJson);
        if (chatHistory.IsNullOrEmpty())
            throw new Exception($"Failed to decode previous chathistory in {nameof(LlmServiceSemanticKernel)}.");

        if (!retrievedContextSources.IsNullOrEmpty())
        {
            var sourcesString = retrievedContextSources.ToSourcesString();
            chatHistory.AddUserMessage(sourcesString);
        }

        if (!chatMessages.IsNullOrEmpty())
            chatHistory.AddToChatHistory(chatMessages);

        var chatResponse = await GetChatResponseInternal(chatHistory, chatRequestOptions);

        chatResponse.Citations = retrievedContextSources.IsNullOrEmpty() ? [] : retrievedContextSources.ToList();

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

            FunctionChoiceBehaviorOptions options = new() { AllowParallelCalls = chatOptions.AllowMultiplePluginCallsPerCompletion };

            openAIPromptExecutionSettings.FunctionChoiceBehavior
                = (chatOptions.PluginUseRequired ?? false)
                    ? FunctionChoiceBehavior.Required(options: options, autoInvoke: chatOptions.PluginsAutoInvoke ?? true)
                    : FunctionChoiceBehavior.Auto(options: options, autoInvoke: chatOptions.PluginsAutoInvoke ?? true);
        }

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var result = await chatCompletionService.GetChatMessageContentAsync(
           chatHistory,
           executionSettings: openAIPromptExecutionSettings,
           kernel: _kernel)
            ?? throw new Exception($"Failed to get chat completion response: Received null response from chat completion service using semantic kernel.");

        if (string.IsNullOrWhiteSpace(result.Content))
            chatHistory.AddAssistantMessage(result.Content);

        //TODO add tool calls to chatHistory

        return new ChatResponse(result.Content)
        {
            ChatHistoryJson = JsonSerializer.Serialize(chatHistory)
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
