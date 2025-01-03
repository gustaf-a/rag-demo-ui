using Shared.Extensions;
using Shared.Models;

namespace Shared.Models;

#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class SearchResponse
{
    private string _message;

    public List<RetrievedDocument> Citations { get; set; } = [];
    public string Intent { get; set; }

    public SearchResponse(string intent, IEnumerable<Azure.AI.OpenAI.Chat.ChatCitation> citations)
    {
        Citations = citations.ToChatMessageCitations();
        Intent = intent;
    }

    public SearchResponse(string message)
    {
        _message = message;
    }

    public SearchResponse(IEnumerable<RetrievedDocument> retrievedSources)
    {
        Citations = retrievedSources.ToList();
    }
}

