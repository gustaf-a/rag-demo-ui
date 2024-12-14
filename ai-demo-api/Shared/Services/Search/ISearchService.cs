using AiDemos.Api.Models;

namespace Shared.Services.Search
{
    public interface ISearchService
    {
        Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest);
        Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(SearchRequest searchRequest);
    }
}