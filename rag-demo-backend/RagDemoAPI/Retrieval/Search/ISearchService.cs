using RagDemoAPI.Models;

namespace RagDemoAPI.Retrieval.Search
{
    public interface ISearchService
    {
        Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest);
    }
}