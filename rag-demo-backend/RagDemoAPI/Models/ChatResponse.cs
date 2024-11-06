using RagDemoAPI.Extensions;

namespace RagDemoAPI.Models;

#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class ChatResponse
{
    public List<ChatMessage> ChatMessages { get; set; } = [];

    public List<RetrievedDocument> Citations { get; set; } = [];
    public string Intent { get; set; }

    public ChatResponse(string text)
    {
        ChatMessages.Add(new ChatMessage(ChatMessageRoles.Assistant, text));
    }

    public ChatResponse(string text, List<RetrievedDocument> sourcesUsed) : this(text)
    {
        Citations = sourcesUsed;
    }

    public ChatResponse(string text, string intent, IEnumerable<Azure.AI.OpenAI.Chat.ChatCitation> citations) : this(text)
    {
        Citations = citations.ToChatMessageCitations();
        Intent = intent;
    }
}

