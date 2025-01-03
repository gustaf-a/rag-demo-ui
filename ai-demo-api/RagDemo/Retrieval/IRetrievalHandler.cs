using Shared.Models;
using Shared.Models;

namespace AiDemos.Api.Retrieval
{
    public interface IRetrievalHandler
    {
        Task<IEnumerable<RetrievedDocument>> DoSearch(SearchRequest searchRequest);
        Task<IEnumerable<ContentChunk>> GetContentChunks(ContentChunkRequest contentChunkRequest);
        Task<IEnumerable<RetrievedDocument>> RetrieveContextForQuery(ChatRequest chatRequest);
    }
}