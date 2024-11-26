﻿using Npgsql;
using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;
using System.Text;
using System.Text.Json;

namespace RagDemoAPI.Repositories;

public class PostgreSqlRepository : IPostgreSqlRepository
{
    private readonly PostgreSqlOptions _options;
    private readonly NpgsqlConnection _connection;

    public PostgreSqlRepository(IConfiguration configuration)
    {
        _options = configuration.GetSection(PostgreSqlOptions.PostgreSql).Get<PostgreSqlOptions>() ?? throw new ArgumentNullException(nameof(PostgreSqlOptions));
        _connection = new NpgsqlConnection(_options.ConnectionString);
    }

    public async Task<IEnumerable<string>> GetTableNames()
    {
        string query = @"
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_type = 'BASE TABLE';
        ";

        var parameters = new Dictionary<string, object>();

        return await ExecuteQueryAsync(
            query,
            parameters,
            reader => reader.GetString(reader.GetOrdinal("table_name"))
        );
    }

    public async Task<IEnumerable<string>> GetUniqueMetaDataTagKeys(DatabaseOptions databaseOptions)
    {
        string query = $@"
            SELECT DISTINCT jsonb_object_keys(metadata->'Tags') AS unique_keys
            FROM {databaseOptions.TableName};
        ";

        return await ExecuteQueryAsync(
            query,
            [],
            reader => reader.GetString(reader.GetOrdinal("unique_keys"))
        );
    }

    public async Task<IEnumerable<string>> GetUniqueMetaDataTagValues(DatabaseOptions databaseOptions, string tag)
    {
        string query = $@"
            SELECT DISTINCT metadata->'Tags'->>'{tag}' AS tag_value
            FROM {databaseOptions.TableName}
            WHERE metadata->'Tags' ? '{tag}';
        ";

        return await ExecuteQueryAsync(
            query,
            [],
            reader => reader.GetString(reader.GetOrdinal("tag_value"))
        );
    }

    public async Task<bool> DoesTableExist(DatabaseOptions databaseOptions)
    {
        var tableNames = await GetTableNames();

        return tableNames.Contains(databaseOptions.TableName);
    }

    public async Task ResetTable(DatabaseOptions databaseOptions)
    {
        await DeleteTable(databaseOptions);

        await CreateEmbeddingsTable(databaseOptions);
    }

    public async Task DeleteTable(DatabaseOptions databaseOptions)
    {
        string dropTableQuery = $"DROP TABLE IF EXISTS {databaseOptions.TableName};";

        await ExecuteQuery(dropTableQuery);
    }

    public async Task CreateEmbeddingsTable(DatabaseOptions databaseOptions)
    {
        string createTableQuery = $@"
            CREATE TABLE IF NOT EXISTS {databaseOptions.TableName} (
                id BIGINT PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY,
                metadata JSONB,
                content TEXT,
                embedding VECTOR({databaseOptions.EmbeddingsDimensions}) NOT NULL
            );";

        await ExecuteQuery(createTableQuery);
    }

    public async Task SetupIndex()
    {
        //TODO create index
        //        const string createDiskAnnQuery =
        //@"
        //CREATE EXTENSION IF NOT EXISTS pg_diskann CASCADE;
        //CREATE INDEX embeddings_diskann_idx ON embeddings USING diskann (embedding vector_cosine_ops);
        //";

        //        await ExecuteQuery(createDiskAnnQuery);
    }

    public async Task InsertData(DatabaseOptions databaseOptions, string content, float[] embedding, EmbeddingMetaData metaData)
    {
        string insertQuery = $@"
            INSERT INTO {databaseOptions.TableName} (content, embedding, metadata)
            VALUES (@content, @embedding, @metadata);";

        var metadataJson = JsonSerializer.Serialize(metaData);

        var embeddingsDouble = Array.ConvertAll(embedding, x => (double)x);

        await _connection.OpenAsync();
        try
        {
            using var command = new NpgsqlCommand(insertQuery, _connection);

            command.Parameters.AddWithValue("@content", content);
            command.Parameters.AddWithValue("@embedding", embeddingsDouble);
            command.Parameters.Add("@metadata", NpgsqlTypes.NpgsqlDbType.Jsonb)
                   .Value = metadataJson;

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task<IEnumerable<Models.RetrievedDocument>> RetrieveData(string embeddingsTableName, PostgreSqlQueryParameters queryParameters)
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        var sqlBuilder = new StringBuilder();
        var whereClauses = new List<string>();
        var parameters = new Dictionary<string, object>();

        sqlBuilder.AppendLine("SELECT *");
        sqlBuilder.AppendLine($"FROM {embeddingsTableName}");

        if (!queryParameters.ContentMustIncludeWords.IsNullOrEmpty())
        {
            for (int i = 0; i < queryParameters.ContentMustIncludeWords.Count(); i++)
                AddColumnFilter(whereClauses, parameters, "content", isInclude: true, paramName: $"@TextQueryIncl{i}", queryParameters.ContentMustIncludeWords.ElementAt(i));
        }

        if (!queryParameters.ContentMustNotIncludeWords.IsNullOrEmpty())
        {
            for (int i = 0; i < queryParameters.ContentMustNotIncludeWords.Count(); i++)
                AddColumnFilter(whereClauses, parameters, "content", isInclude: false, paramName: $"@TextQueryExcl{i}", queryParameters.ContentMustNotIncludeWords.ElementAt(i));
        }

        if (!queryParameters.MetaDataFilterInclude.IsNullOrEmpty())
            whereClauses.AddRange(queryParameters.MetaDataFilterInclude);

        if (!queryParameters.MetaDataFilterExclude.IsNullOrEmpty())
            whereClauses.Add($"NOT ({string.Join(" OR ", queryParameters.MetaDataFilterExclude)})");

        if (whereClauses.Any())
            sqlBuilder.AppendLine("WHERE " + string.Join(" AND ", whereClauses));

        var embeddingsString = $"[{string.Join(',', queryParameters.EmbeddingQuery.Select(e => e.ToString()))}]";
        sqlBuilder.AppendLine($"ORDER BY embedding <=> '{embeddingsString}'");

        if (queryParameters.ItemsToSkip > 0)
        {
            sqlBuilder.AppendLine("LIMIT @ItemsToRetrieve OFFSET @ItemsToSkip");
            parameters.Add("@ItemsToRetrieve", queryParameters.ItemsToRetrieve);
            parameters.Add("@ItemsToSkip", queryParameters.ItemsToSkip);
        }
        else
        {
            sqlBuilder.AppendLine("LIMIT @ItemsToRetrieve");
            parameters.Add("@ItemsToRetrieve", queryParameters.ItemsToRetrieve);
        }

        var retrieveDataQuery = sqlBuilder.ToString();

        var queryResult = await ExecuteQueryAsync(
            retrieveDataQuery,
            parameters,
            CreateEmbeddingsRowModelMapFunction
        );

        if (queryResult == null || !queryResult.Any())
            return Enumerable.Empty<Models.RetrievedDocument>();

        return queryResult.Select(qr => new Models.RetrievedDocument(embeddingsTableName, qr));
    }

    private static void AddColumnFilter(List<string> whereClauses, Dictionary<string, object> parameters, string tableColumn, bool isInclude, string paramName, string word)
    {
        whereClauses.Add($"{(isInclude ? "" : "NOT ")}{tableColumn} ILIKE '%' || {paramName} || '%'");
        parameters.Add(paramName, word);
    }

    private static EmbeddingsRowModel CreateEmbeddingsRowModelMapFunction(NpgsqlDataReader reader)
    {
        var metadataJson = reader.GetString(reader.GetOrdinal("metadata")); // JSONB as JSON string

        var metadata = JsonSerializer.Deserialize<EmbeddingMetaData>(metadataJson);

        return new EmbeddingsRowModel
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            Metadata = metadata,
            Content = reader.GetString(reader.GetOrdinal("content")),
            EmbeddingVector = null //Not relevant during retrieval
        };
    }

    private async Task ExecuteQuery(string query)
    {
        await _connection.OpenAsync();
        try
        {
            using var command = new NpgsqlCommand(query, _connection);
            var affected = await command.ExecuteNonQueryAsync();
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    private async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string query, Dictionary<string, object> parameters, Func<NpgsqlDataReader, T> mapFunction)
    {
        var results = new List<T>();
        await _connection.OpenAsync();
        try
        {
            using var command = new NpgsqlCommand(query, _connection);

            foreach (var parameter in parameters)
                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(mapFunction(reader));
            }
        }
        finally
        {
            await _connection.CloseAsync();
        }
        return results;
    }
}
