using Shared.Extensions;
using Shared.Models;

namespace Shared.Models;

#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class ChatResponse
{
    public List<ChatMessage> ChatMessages { get; set; } = [];

    public List<RetrievedDocument> Citations { get; set; } = [];

    /// <summary>
    /// Contains the original question and tool calls
    /// </summary>
    public string ChatHistoryJson { get; set; }

    public ChatResponse()
    {
    }

    public ChatResponse(string text)
    {
        if(!string.IsNullOrWhiteSpace(text))
            ChatMessages.Add(new ChatMessage(ChatMessageRoles.Assistant, text));
    }

    public ChatResponse(string text, List<RetrievedDocument> sourcesUsed) : this(text)
    {
        Citations = sourcesUsed;
    }

    public ChatResponse(string text, IEnumerable<Azure.AI.OpenAI.Chat.ChatCitation> citations) : this(text)
    {
        Citations = citations.ToChatMessageCitations();
    }
}

