namespace Shared.Configuration;

public class PostgreSqlOptions
{
    public const string PostgreSql = "PostgreSql";

    public string ConnectionString { get; set; } = string.Empty;
    public string DefaultEmbeddingsTableName { get; set; } = "embeddings2";
}
