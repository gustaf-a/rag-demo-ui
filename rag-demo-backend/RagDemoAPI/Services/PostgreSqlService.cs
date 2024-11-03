
using Npgsql;
using RagDemoAPI.Configuration;

namespace RagDemoAPI.Services;

public class PostgreSqlService : IPostgreSqlService
{
    private readonly PostgreSqlOptions _options;
    private readonly NpgsqlConnection _connection;

    public PostgreSqlService(IConfiguration configuration)
    {
        _options = configuration.GetSection(PostgreSqlOptions.PostgreSql).Get<PostgreSqlOptions>() ?? throw new ArgumentNullException(nameof(PostgreSqlOptions));
        _connection = new NpgsqlConnection(_options.ConnectionString);
    }

    public async Task ResetDatabaseAsync()
    {
        const string dropTableQuery = "DROP TABLE IF EXISTS embeddings;";

        await _connection.OpenAsync();
        try
        {
            using (var dropCommand = new NpgsqlCommand(dropTableQuery, _connection))
            {
                await dropCommand.ExecuteNonQueryAsync();
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }

        await SetupTablesAsync();
    }

    public async Task SetupTablesAsync()
    {
        const string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS embeddings (
                id SERIAL PRIMARY KEY,
                metadata TEXT NOT NULL,
                content TEXT NOT NULL,
                embedding FLOAT8[] NOT NULL
            );";

        await _connection.OpenAsync();
        try
        {
            using var command = new NpgsqlCommand(createTableQuery, _connection);
            var affected = await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task InsertDataAsync(string content, float[] embedding)
    {
        const string insertQuery = @"
            INSERT INTO embeddings (content, embedding)
            VALUES (@content, @embedding);";

        await _connection.OpenAsync();
        try
        {
            using var command = new NpgsqlCommand(insertQuery, _connection);
            command.Parameters.AddWithValue("@content", content);
            command.Parameters.AddWithValue("@embedding", embedding);
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
}
