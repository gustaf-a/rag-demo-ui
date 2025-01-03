using Microsoft.SemanticKernel.ChatCompletion;
using Shared.Extensions;
using Shared.Models;

namespace Shared.Extensions;

public static class ChatHistoryExtensions
{
    public static Microsoft.SemanticKernel.ChatCompletion.ChatHistory AddToChatHistory(this Microsoft.SemanticKernel.ChatCompletion.ChatHistory chatHistory, IEnumerable<ChatMessage> chatMessages)
    {
        if (chatMessages.IsNullOrEmpty())
            return chatHistory;

        var semanticKernelChatMessage = chatMessages.ToSemanticKernelChatMessages();

        foreach (var skChatMessage in semanticKernelChatMessage)
            chatHistory.Add(skChatMessage);

        return chatHistory;
    }

    public static string GetLastFinishReason(this ChatHistory chatHistory)
    {
        var lastChatContent = chatHistory.Last();
        if (lastChatContent is null || lastChatContent.Metadata.IsNullOrEmpty())
            return string.Empty;


        if (!lastChatContent.Metadata.TryGetValue("FinishReason", out var finishReason))
            return string.Empty;

        return finishReason.ToString();
    }

    public static bool LastMessageWasFrom(this ChatHistory chatHistory, string roleLabel)
    {
        var lastChatContent = chatHistory.Last() 
            ?? throw new Exception("Failed to find last message in chatHistory.");

        if (string.IsNullOrWhiteSpace(lastChatContent.Role.Label))
            throw new Exception($"Last message in chat history contains no valid role.");

        return roleLabel.Equals(lastChatContent.Role.Label, StringComparison.InvariantCultureIgnoreCase);
    }
}
