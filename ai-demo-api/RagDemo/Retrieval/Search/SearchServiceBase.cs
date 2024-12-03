using AiDemos.Api.Extensions;
using AiDemos.Api.Generation.LlmServices;
using AiDemos.Api.Models;
using System.Text;

namespace AiDemos.Api.Retrieval.Search;

public abstract class SearchServiceBase(ILlmServiceFactory _llmServiceFactory)
{
    protected async Task<string> GetQueryContentFromChatMessages(IEnumerable<ChatMessage> chatMessages, SearchOptions searchOptions)
    {
        if (chatMessages.IsNullOrEmpty())
            return string.Empty;

        if (searchOptions.SemanticSearchGenerateSummaryOfNMessages <= 0)
            return chatMessages.Last().Content;

        var chatMessagesToSummarize = chatMessages.ToList().TakeLastOrAll(searchOptions.SemanticSearchGenerateSummaryOfNMessages);
        var chatMessageRows = chatMessagesToSummarize.ToRows();

        var querySb = new StringBuilder();

        querySb.AppendLine("<chatMessages>");

        foreach (var chatMessageRow in chatMessageRows)
            querySb.AppendLine(chatMessageRow);

        querySb.AppendLine("</chatMessages>");
        
        querySb.AppendLine();

        var summarizeMessagesQuery =
"""
Your task is to generate a keywords of the chat messages which will be used to find the relevant information.
Focus on the last question from the user and only include information from the other messages which is relevant to the latest chat message.
Only return the keywords and nothing else.
""";

        querySb.AppendLine(summarizeMessagesQuery);

        var llmService = _llmServiceFactory.Create(searchOptions);

        var generatedSummary = await llmService.GetCompletionSimple(querySb.ToString());

        return generatedSummary;
    }
}