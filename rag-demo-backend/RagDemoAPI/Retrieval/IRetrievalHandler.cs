using RagDemoAPI.Models;

namespace RagDemoAPI.Retrieval
{
    public interface IRetrievalHandler
    {
        Task<IEnumerable<RetrievedDocument>> DoSearch(SearchRequest searchRequest);
        Task<IEnumerable<RetrievedDocument>> RetrieveContextForQuery(ChatRequest chatRequest);
    }
}