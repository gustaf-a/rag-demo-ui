namespace AiDemos.Api.Models;

public class RetrievedDocument
{
    public RetrievedDocument()
    {
    }
    
    public RetrievedDocument(string tableName, EmbeddingsRowModel embeddingsRowModel)
    {
        TableName = tableName;
        Content = embeddingsRowModel.Content;
        EmbeddingContent = embeddingsRowModel.EmbeddingContent;
        StartIndex = embeddingsRowModel.StartIndex;
        EndIndex = embeddingsRowModel.EndIndex;
        ChunkId = embeddingsRowModel.Id.ToString();
        MetaData = embeddingsRowModel.Metadata;
        Title = Path.GetFileName(embeddingsRowModel.Metadata.Uri);
        Uri = new Uri(embeddingsRowModel.Metadata.Uri);
    }

    public RetrievedDocument(string sourceUri, string content)
    {
        Uri = new Uri(sourceUri);
        Content = content;
    }

    public Uri Uri { get; set; }
    public string TableName { get; }
    public string Content { get; set; }
    public string EmbeddingContent { get; }
    public long StartIndex { get; }
    public long EndIndex { get; }
    public string Title { get; set; }
    public EmbeddingMetaData? MetaData { get; }
    public string ChunkId { get; set; }
    public double? RerankScore { get; set; }
    public IDictionary<string, string> AdditionalData { get; set; }

    public string ContentAfter { get; set; }
    public string ContentBefore { get; set; }

    public override string ToString()
    {
        return $"{Title}: {Content}";
    }
}

