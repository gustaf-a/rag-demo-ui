using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using AiDemos.Api.Extensions;
using AiDemos.Api.Models;
using Shared.Plugins;
using System.Text.Json;
using ChatMessage = AiDemos.Api.Models.ChatMessage;
using ChatOptions = AiDemos.Api.Models.ChatOptions;

namespace AiDemos.Api.Generation.LlmServices;

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

    public async Task<ChatResponse> ContinueChatResponse(string previousChatHistoryJson, IEnumerable<ChatMessage> chatMessages, IEnumerable<RetrievedDocument> retrievedContextSources, ChatOptions chatRequestOptions)
    {
        if (string.IsNullOrWhiteSpace(previousChatHistoryJson))
            throw new Exception($"Cannot continue chat if previous chat history is nothing.");

        var chatHistory = JsonSerializer.Deserialize<ChatHistory>(previousChatHistoryJson);
        if (chatHistory.IsNullOrEmpty())
            throw new Exception($"Failed to decode previous chathistory in {nameof(LlmServiceSemanticKernel)}.");

        if (!chatMessages.IsNullOrEmpty())
            chatHistory.AddToChatHistory(chatMessages);

        if (chatHistory.LastMessageWasFrom("Assistant"))
        {
            var finishReason = chatHistory.GetLastFinishReason();
            if ("stop".Equals(finishReason, StringComparison.InvariantCulture) || "completed".Equals(finishReason, StringComparison.InvariantCulture))
            {
                //finished. Return nothing.
                return new ChatResponse();
            }
            else if ("ToolCalls".Equals(finishReason, StringComparison.InvariantCulture))
            {
                //if auto, continue
                var chatHistoryFromToolCall = await DoToolCalls(_kernel, _pluginHandler, chatMessages, chatHistory);

                if (!(chatRequestOptions.PluginsAutoInvoke ?? true))
                {
                    return new ChatResponse
                    {
                        ChatHistoryJson = JsonSerializer.Serialize(chatHistoryFromToolCall)
                    };
                }

                //Continue as normal
                chatHistory = chatHistoryFromToolCall;
            }
            else
            {
                //Not finished. Continue as if is user message.
            }
        }

        if (!retrievedContextSources.IsNullOrEmpty())
        {
            var sourcesString = retrievedContextSources.ToSourcesString();
            chatHistory.AddUserMessage(sourcesString);
        }

        var chatResponse = await GetChatResponseInternal(chatHistory, chatRequestOptions);

        chatResponse.Citations = retrievedContextSources.IsNullOrEmpty() ? [] : retrievedContextSources.ToList();

        return chatResponse;
    }

    private static async Task<ChatHistory> DoToolCalls(Kernel _kernel, IPluginHandler _pluginHandler, IEnumerable<ChatMessage> chatMessages, ChatHistory chatHistory)
    {
        var functionCallsInLastChatMessage = FunctionCallContent.GetFunctionCalls(chatHistory.Last());
        if (functionCallsInLastChatMessage.IsNullOrEmpty())
            throw new Exception($"Last message finish reason was tool call, but no function calls found.");

        if (!chatMessages.IsNullOrEmpty())
            throw new Exception($"Cannot execute function call and handle user message at the same time. Please first finish function calling sequence.");

        _pluginHandler.AddPlugins(_kernel);

        foreach (var functionCall in functionCallsInLastChatMessage)
        {
            try
            {
                var resultContent = await functionCall.InvokeAsync(_kernel);

                chatHistory.Add(resultContent.ToChatMessage());
            }
            catch (Exception ex)
            {
                chatHistory.Add(new FunctionResultContent(functionCall, ex).ToChatMessage());
            }
        }

        return chatHistory;
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

        if (!string.IsNullOrWhiteSpace(result.Content))
            chatHistory.AddMessage(AuthorRole.Assistant, result.Content, metadata: result.Metadata);

        //If not auto need to add to history.
        //If auto invoke, they will be added automatically to chathistory.
        if (!(chatOptions.PluginsAutoInvoke ?? true))
        {
            var functionCalls = FunctionCallContent.GetFunctionCalls(result);
            if (!functionCalls.IsNullOrEmpty())
                chatHistory.Add(result);
        }

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
