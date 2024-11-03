namespace RagDemoAPI.Models;

public class RetrievedDocument
{
    public RetrievedDocument()
    {
    }

    public RetrievedDocument(string sourceUri, string content)
    {
        Uri = new Uri(sourceUri);
        Content = content;
    }

    public Uri Uri { get; set; }
    public string Content { get; set; }
    public string Title { get; set; }
    public string ChunkId { get; set; }
    public double? RerankScore { get; set; }
    public IDictionary<string, string> AdditionalData { get; set; }

    public override string ToString()
    {
        return $"{Title}: {Content}";
    }
}

