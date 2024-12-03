using AiDemos.Api.Models;

namespace AiDemos.Api.Retrieval
{
    public interface IRetrievalHandler
    {
        Task<IEnumerable<RetrievedDocument>> DoSearch(SearchRequest searchRequest);
        Task<IEnumerable<RetrievedDocument>> RetrieveContextForQuery(ChatRequest chatRequest);
    }
}