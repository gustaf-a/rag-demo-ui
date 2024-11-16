using RagDemoAPI.Models;

namespace RagDemoAPI.Extensions;

public static class ChatMessageCitationExtensions
{
#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public static List<RetrievedDocument> ToChatMessageCitations(this IEnumerable<Azure.AI.OpenAI.Chat.ChatCitation> azureChatCitations)
    {
        var citations = new List<RetrievedDocument>();

        foreach (var azureChatCitation in azureChatCitations)
        {
            citations.Add(new RetrievedDocument
            {
                Title = azureChatCitation.Title,
                Content = azureChatCitation.Content,
                Uri = azureChatCitation.Uri,
                ChunkId = azureChatCitation.ChunkId,
                RerankScore = azureChatCitation.RerankScore
            });
        }

        return citations;
    }
}
