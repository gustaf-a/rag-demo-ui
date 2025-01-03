namespace Shared.Models;

public class EmbeddingsRowModel
{
    public long Id { get; set; }

    public EmbeddingMetaData Metadata { get; set; }

    public string Content { get; set; }
    public string EmbeddingContent { get; set; }
    public double[] EmbeddingVector { get; set; }
    public long StartIndex { get; set; }
    public long EndIndex { get; set; }
}
