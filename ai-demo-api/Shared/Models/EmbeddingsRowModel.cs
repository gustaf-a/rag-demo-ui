namespace AiDemos.Api.Models;

public class EmbeddingsRowModel
{
    public long Id { get; set; }

    public EmbeddingMetaData Metadata { get; set; }

    public string Content { get; set; }

    public double[] EmbeddingVector { get; set; }
}
