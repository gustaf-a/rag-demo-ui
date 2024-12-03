using AiDemos.Api.Models;
using System.Text;

namespace AiDemos.Api.Extensions;

public static class RetrievedDocumentExtensions
{
#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public static List<Models.RetrievedDocument> ToChatMessageCitations(this IEnumerable<Azure.AI.OpenAI.Chat.ChatCitation> azureChatCitations)
    {
        var citations = new List<Models.RetrievedDocument>();

        foreach (var azureChatCitation in azureChatCitations)
        {
            citations.Add(new Models.RetrievedDocument
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

    public static string ToSourcesString(this IEnumerable<RetrievedDocument> retrievedDocuments)
    {
        if (retrievedDocuments.IsNullOrEmpty())
        {
            return null;
        }

        var sb = new StringBuilder();

        foreach (var document in retrievedDocuments)
            sb.AppendLine(document.ToString());

        return
$"""
<sources to use>
{sb}
</sources>
""";
    }
}
