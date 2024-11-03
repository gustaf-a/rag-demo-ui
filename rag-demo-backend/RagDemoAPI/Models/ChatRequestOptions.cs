namespace RagDemoAPI.Models
{
    public class ChatRequestOptions
    {
        public RetrievalOptions GlobalSearchRetrievalOptions { get; set; } = new();

        public bool UseTextSearch { get; set; }
        public RetrievalOptions TextSearchRetrievalOptions { get; set; } = new();

        public bool UseVectorSearch { get; set; }
        public RetrievalOptions VectorSearchRetrievalOptions { get; set; } = new();
        public double? Temperature { get; set; } = 0.3;
    }
}
