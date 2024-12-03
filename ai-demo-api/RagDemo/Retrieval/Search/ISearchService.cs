using AiDemos.Api.Models;

namespace AiDemos.Api.Retrieval.Search
{
    public interface ISearchService
    {
        Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest);
        Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(SearchRequest searchRequest);
    }
}