using RagDemoAPI.Models;

namespace RagDemoAPI.Retrieval
{
    public interface IRetrievalHandler
    {
        Task<IEnumerable<RetrievedDocument>> RetrieveContextForQuery(ChatRequest chatRequest);
    }
}