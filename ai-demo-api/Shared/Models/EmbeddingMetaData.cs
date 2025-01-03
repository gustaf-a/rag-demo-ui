namespace Shared.Models;

public class EmbeddingMetaData
{
    public string Uri { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public string Source { get; set; }
    public int SourceChunkNumber { get; set; }
    public int SourceTotalChunkNumbers { get; set; }
    public Dictionary<string, string> Tags { get; set; }
}
