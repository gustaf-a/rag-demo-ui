namespace RagDemoAPI.Extensions;

public static class ChatHistoryExtensions
{
    public static Microsoft.SemanticKernel.ChatCompletion.ChatHistory AddToChatHistory(this Microsoft.SemanticKernel.ChatCompletion.ChatHistory chatHistory, IEnumerable<Models.ChatMessage> chatMessages)
    {
        if (chatMessages.IsNullOrEmpty())
            return chatHistory;

        var semanticKernelChatMessage = chatMessages.ToSemanticKernelChatMessages();

        foreach (var skChatMessage in semanticKernelChatMessage)
            chatHistory.Add(skChatMessage);

        return chatHistory;
    }
}
