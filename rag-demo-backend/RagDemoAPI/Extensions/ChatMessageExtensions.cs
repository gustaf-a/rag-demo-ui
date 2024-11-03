using RagDemoAPI.Models;
using System.Text;

namespace RagDemoAPI.Extensions;

public static class ChatMessageExtensions
{
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

    public static List<OpenAI.Chat.ChatMessage> ToOpenAiChatMessages(this IEnumerable<ChatMessage> messages, IEnumerable<RetrievedDocument> retrievedDocuments)
    {
        var chatHistory = messages.ToOpenAiChatMessages();

        if (!retrievedDocuments.Any())
            return chatHistory;

        var sb = new StringBuilder();

        foreach (var document in retrievedDocuments)
        {
            sb.AppendLine(document.ToString());
            sb.AppendLine();
        }

        chatHistory.Add(new OpenAI.Chat.UserChatMessage(
$"""
<sources>
{sb}
</sources>
"""));

        return chatHistory;
    }

    public static Microsoft.SemanticKernel.ChatCompletion.ChatHistory ToSemanticKernelChatMessages(this IEnumerable<Models.ChatMessage> messages)
    {
        var chatHistory = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();

        foreach (var message in messages)
        {
            if (string.Equals(message.Role, ChatMessageRoles.User, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.AddUserMessage(message.Content);
            else if (string.Equals(message.Role, ChatMessageRoles.System, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.AddSystemMessage(message.Content);
            else if (string.Equals(message.Role, ChatMessageRoles.Assistant, StringComparison.InvariantCultureIgnoreCase))
                chatHistory.AddAssistantMessage(message.Content);
            else
                throw new NotSupportedException($"Unsupported chat message role encountered: {message.Role}");
        }

        return chatHistory;
    }

    public static Microsoft.SemanticKernel.ChatCompletion.ChatHistory ToSemanticKernelChatMessages(this IEnumerable<ChatMessage> messages, IEnumerable<RetrievedDocument> retrievedDocuments)
    {
        var chatHistory = messages.ToSemanticKernelChatMessages();

        var sb = new StringBuilder();

        foreach (var document in retrievedDocuments)
            sb.AppendLine(document.ToString());

        chatHistory.AddUserMessage(
$"""
<sources to use>
{sb}
</sources>
""");

        return chatHistory;
    }
}
