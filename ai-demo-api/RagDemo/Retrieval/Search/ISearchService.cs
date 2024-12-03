using RagDemoAPI.Models;

namespace RagDemoAPI.Retrieval.Search
{
    public interface ISearchService
    {
        Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest);
        Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(SearchRequest searchRequest);
    }
}