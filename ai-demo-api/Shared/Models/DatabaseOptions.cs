namespace Shared.Models;

public class DatabaseOptions
{
    public string TableName { get; set; } = "embeddings";
    public int? EmbeddingsDimensions { get; set; } = 3072;
}
