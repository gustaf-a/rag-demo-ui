using Microsoft.SemanticKernel.ChatCompletion;
using RagDemoAPI.Models;

namespace RagDemoAPI.Extensions;

public static class ChatMessageExtensions
{
    public static IEnumerable<string> ToRows(this IEnumerable<ChatMessage> messages)
    {
        return messages.Select(m => $"{m.Role}: {m.Content}");
    }

    public static List<OpenAI.Chat.ChatMessage> ToOpenAiChatMessages(this IEnumerable<ChatMessage> messages)
    {
        var chatHistory = new List<OpenAI.Chat.ChatMessage>();

        foreach (var message in messages)
        {
            if (string.Equals(message.Role, ChatMessageRoles.User, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.Add(new OpenAI.Chat.UserChatMessage(message.Content));
            else if (string.Equals(message.Role, ChatMessageRoles.System, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.Add(new OpenAI.Chat.SystemChatMessage(message.Content));
            else if (string.Equals(message.Role, ChatMessageRoles.Assistant, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.Add(new OpenAI.Chat.AssistantChatMessage(message.Content));
            else
                throw new NotSupportedException($"Unsupported chat message role encountered: {message.Role}");
        }

        return chatHistory;
    }

    public static List<OpenAI.Chat.ChatMessage> ToOpenAiChatMessages(this IEnumerable<ChatMessage> messages, IEnumerable<Models.RetrievedDocument> retrievedDocuments)
    {
        var chatHistory = messages.ToOpenAiChatMessages();

        if (!retrievedDocuments.Any())
            return chatHistory;

        var sourcesString = retrievedDocuments.ToSourcesString();

        chatHistory.Insert(0, new OpenAI.Chat.UserChatMessage(
$"""
<sources>
{sourcesString}
</sources>
"""));

        return chatHistory;
    }

    public static ChatHistory ToSemanticKernelChatMessages(this IEnumerable<Models.ChatMessage> messages)
    {
        var chatHistory = new ChatHistory();

        foreach (var message in messages)
        {
            if (string.Equals(message.Role, ChatMessageRoles.User, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.AddUserMessage(message.Content);
            else if (string.Equals(message.Role, ChatMessageRoles.System, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.AddSystemMessage(message.Content);
            else if (string.Equals(message.Role, ChatMessageRoles.Assistant, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.AddAssistantMessage(message.Content);
            else if (string.Equals(message.Role, ChatMessageRoles.Tool, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.AddMessage(AuthorRole.Tool, message.Content);
            else
                throw new NotSupportedException($"Unsupported chat message role encountered: {message.Role}");
        }

        return chatHistory;
    }

    public static ChatHistory ToSemanticKernelChatMessages(this IEnumerable<ChatMessage> messages, IEnumerable<Models.RetrievedDocument> retrievedDocuments)
    {
        var chatHistory = messages.ToSemanticKernelChatMessages();

        if (!retrievedDocuments.IsNullOrEmpty())
        {
            var sourcesString = retrievedDocuments.ToSourcesString();
            chatHistory.AddUserMessage(sourcesString);
        }

        return chatHistory;
    }
}
