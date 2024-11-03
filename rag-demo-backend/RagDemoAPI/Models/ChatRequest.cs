namespace RagDemoAPI.Models
{
    public class ChatRequest
    {
        public IEnumerable<ChatMessage> ChatMessages { get; set; }
        public ChatRequestOptions ChatRequestOptions { get; set; } = new();
        public IEnumerable<RetrievedDocument> ProvidedDocumentSources { get; set; }
    }
}
