using Microsoft.SemanticKernel.ChatCompletion;
using Shared.Models;
using Microsoft.SemanticKernel;

namespace Shared.Extensions;

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

    public static List<OpenAI.Chat.ChatMessage> ToOpenAiChatMessages(this IEnumerable<ChatMessage> messages, IEnumerable<RetrievedDocument> retrievedDocuments)
    {
        var chatHistory = messages.ToOpenAiChatMessages();

        if (retrievedDocuments.IsNullOrEmpty())
            return chatHistory;

        var sourcesString = retrievedDocuments.ToSourcesString();

        chatHistory.Insert(0, new OpenAI.Chat.UserChatMessage(
$"""
Please use the following sources when answering the question.
If you use a source, please refer to it in the answer.

{sourcesString}
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

    public static ChatHistory ToSemanticKernelChatMessages(this IEnumerable<ChatMessage> messages, IEnumerable<RetrievedDocument> retrievedDocuments)
    {
        var chatHistory = messages.ToSemanticKernelChatMessages();

        if (!retrievedDocuments.IsNullOrEmpty())
        {
            var sourcesString = retrievedDocuments.ToSourcesString();
            chatHistory.AddUserMessage(sourcesString);
        }

        return chatHistory;
    }

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public static ChatMessageContent ToSemanticKernelChatMessageContent(this ChatMessage message)
    {
        AuthorRole role;

        if (string.Equals(message.Role, ChatMessageRoles.User, StringComparison.InvariantCultureIgnoreCase))
            role = AuthorRole.User;
        else if (string.Equals(message.Role, ChatMessageRoles.System, StringComparison.InvariantCultureIgnoreCase))
            role = AuthorRole.System;
        else if (string.Equals(message.Role, ChatMessageRoles.Assistant, StringComparison.InvariantCultureIgnoreCase))
            role = AuthorRole.Assistant;
        else if (string.Equals(message.Role, ChatMessageRoles.Tool, StringComparison.InvariantCultureIgnoreCase))
            role = AuthorRole.Tool;
        else
            throw new NotSupportedException($"Unsupported chat message role encountered: {message.Role}");

        var chatMessage = new ChatMessageContent(role, message.Content);

        return chatMessage;
    }

    public static ChatMessage ToChatMessage(this ChatMessageContent content)
    {
        string role;

        if (content.Role == AuthorRole.User)
            role = ChatMessageRoles.User;
        else if (content.Role == AuthorRole.System)
            role = ChatMessageRoles.System;
        else if (content.Role == AuthorRole.Assistant)
            role = ChatMessageRoles.Assistant;
        else if (content.Role == AuthorRole.Tool)
            role = ChatMessageRoles.Tool;
        else
            throw new NotSupportedException($"Unsupported chat message role encountered: {content.Role}");

        var chatMessage = new ChatMessage(role, content.Content);

        return chatMessage;
    }
}
