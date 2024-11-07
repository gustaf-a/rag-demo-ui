﻿using Npgsql;
using RagDemoAPI.Configuration;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

    //TODO Initialize DB? Or add to readme. CREATE EXTENSION IF NOT EXISTS vectorscale CASCADE;

    public async Task ResetDatabase()
    {
        const string dropTableQuery = "DROP TABLE IF EXISTS embeddings;";

        await ExecuteQuery(dropTableQuery);

        await SetupTables();
    }

    public async Task SetupTables()
    {
        //TODO Replace embedding type with:
        //embedding VECTOR(1536)
        //dynamic, but drive by what? which config? EmbeddingService prob?

        const string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS embeddings (
                id BIGINT PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY,
                metadata JSONB,
                content TEXT,
                embedding VECTOR(3072) NOT NULL
            );";
        //TODO set correct number of 

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

    public async Task InsertData(string content, float[] embedding, EmbeddingMetaData metaData)
    {
        const string insertQuery = @"
            INSERT INTO embeddings (content, embedding, metadata)
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

    public async Task<IEnumerable<RetrievedDocument>> RetrieveData(PostgreSqlQueryParameters queryParameters)
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        var sqlBuilder = new StringBuilder();
        var whereClauses = new List<string>();
        var parameters = new Dictionary<string, object>();

        sqlBuilder.AppendLine("SELECT *");
        sqlBuilder.AppendLine("FROM embeddings");

        if (!string.IsNullOrWhiteSpace(queryParameters.TextQuery))
        {
            whereClauses.Add("to_tsvector('english', text_column) @@ plainto_tsquery('english', @TextQuery)");
            parameters.Add("@TextQuery", queryParameters.TextQuery);
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.MetaDataSearchQuery))
        {
            whereClauses.Add("(metadata::jsonb @> @MetaDataQuery::jsonb)");
            parameters.Add("@MetaDataQuery", queryParameters.MetaDataSearchQuery);
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
        try
        {
            var queryResult = await ExecuteQueryAsync(
                retrieveDataQuery,
                parameters,
                CreateEmbeddingsRowModelMapFunction
            );

            if (queryResult == null || !queryResult.Any())
                return Enumerable.Empty<RetrievedDocument>();

            return queryResult.Select(qr => new RetrievedDocument(qr));
        }
        catch (Exception ex)
        {
            var exMess = ex.Message;
        }
        return null;
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
