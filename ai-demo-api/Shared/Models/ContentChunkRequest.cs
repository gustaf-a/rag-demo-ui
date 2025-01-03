namespace Shared.Models;

public class ContentChunkRequest
{
    public string TableName { get; set; }
    public Uri Uri { get; set; }
    public int StartChunk { get; set; }
    public int EndChunk { get; set; }
}