namespace RagDemoAPI.Models
{
    public class ChatRequest
    {
        public IEnumerable<ChatMessage>? ChatMessages { get; set; }
        public IEnumerable<RetrievedDocument>? ProvidedDocumentSources { get; set; }
        public ChatOptions ChatOptions { get; set; } = new();

        /// <summary>
        /// Adding search is optional, no default is needed
        /// </summary>
        public SearchOptions? SearchOptions { get; set; }
    }
}
