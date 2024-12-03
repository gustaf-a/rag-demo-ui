namespace AiDemos.Api.Configuration;

public class PostgreSqlOptions
{
    public const string PostgreSql = "PostgreSql";

    public string ConnectionString { get; set; } = string.Empty;
}
