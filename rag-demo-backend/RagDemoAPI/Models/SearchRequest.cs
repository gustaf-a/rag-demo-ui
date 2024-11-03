namespace RagDemoAPI.Models
{
    public class SearchRequest
    {
        public string SearchQuery { get; set; }
        public RetrievalOptions VectorSearchRetrievalOptions { get; set; } = new();
    }
}
