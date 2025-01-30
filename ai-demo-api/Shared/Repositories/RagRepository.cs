﻿using Microsoft.Extensions.Configuration;
using Npgsql;
using Shared.Models;
using System.Text;
using System.Text.Json;
using Shared.Extensions;

namespace Shared.Repositories;

public class RagRepository : RepositoryBase, IRagRepository
{
    public RagRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IEnumerable<string>> GetTableNames()
    {
        string query = @"
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_type = 'BASE TABLE';
        ";

        return await ExecuteQueryAsync<string>(
            query,
            []
        );
    }

    public async Task<IEnumerable<string>> GetUniqueMetaDataTagKeys(string tableName)
    {
        string query = $@"
            SELECT DISTINCT jsonb_object_keys(metadata->'Tags') AS unique_keys
            FROM {tableName};
        ";

        return await ExecuteQueryAsync<string>(
            query,
            []
        );
    }

    public async Task<IEnumerable<string>> GetUniqueMetaDataTagValues(string tableName, string tag)
    {
        string query = $@"
            SELECT DISTINCT metadata->'Tags'->>'{tag}' AS tag_value
            FROM {tableName}
            WHERE metadata->'Tags' ? '{tag}';
        ";

        return await ExecuteQueryAsync<string>(
            query,
            []
        );
    }

    public async Task ResetTable(DatabaseOptions databaseOptions)
    {
        await DeleteTable(databaseOptions.TableName);

        await CreateEmbeddingsTable(databaseOptions);
    }

    public async Task CreateEmbeddingsTable(DatabaseOptions databaseOptions)
    {
        string createTableQuery = $@"
            CREATE TABLE IF NOT EXISTS {databaseOptions.TableName} (
                id BIGINT PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY,
                metadata JSONB,
                content TEXT,
                embeddingContent TEXT,
                startIndex BIGINT,
                endIndex BIGINT,
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

    public async Task InsertData(string tableName, ContentChunk contentChunk, float[] embedding, EmbeddingMetaData metaData)
    {
        string insertQuery = $@"
            INSERT INTO {tableName} (content, embedding, embeddingContent, startIndex, endIndex, metadata)
            VALUES (@content, @embedding, @embeddingContent, @startIndex, @endIndex, @metadata);";

        var metadataJson = JsonSerializer.Serialize(metaData);

        var embeddingsDouble = Array.ConvertAll(embedding, x => (double)x);

        using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(insertQuery, connection);

        command.Parameters.AddWithValue("@content", contentChunk.Content);
        command.Parameters.AddWithValue("@embeddingContent", contentChunk.EmbeddingContent);
        command.Parameters.AddWithValue("@startIndex", contentChunk.StartIndex);
        command.Parameters.AddWithValue("@endIndex", contentChunk.EndIndex);
        command.Parameters.AddWithValue("@embedding", embeddingsDouble);
        command.Parameters.Add("@metadata", NpgsqlTypes.NpgsqlDbType.Jsonb)
               .Value = metadataJson;

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<ContentChunk>> GetContentChunks(ContentChunkRequest contentChunkRequest)
    {
        var parameters = new Dictionary<string, object>
        {
            { "start_chunk", contentChunkRequest.StartChunk },
            { "end_chunk", contentChunkRequest.EndChunk },
            { "uri", contentChunkRequest.Uri.ToString() }
        };

        var query = $@"
                        SELECT *
                        FROM {contentChunkRequest.TableName.ToLower()}
                        WHERE (metadata->>'SourceChunkNumber')::int BETWEEN @start_chunk AND @end_chunk
                        AND metadata->>'Uri' = @uri
                    ";

        var queryResult = await ExecuteQueryAsync(
                            query,
                            parameters,
                            CreateEmbeddingsRowModelMapFunction
                          );

        if (queryResult == null || !queryResult.Any())
            return Enumerable.Empty<ContentChunk>();

        return queryResult.Select(qr => new ContentChunk(qr));
    }

    public async Task<IngestionSource> GetIngestionSource(Guid id)
    {
        const string query = @"
        SELECT Id, Name, Content, Metadata
        FROM IngestionSources
        WHERE Id = @Id
        LIMIT 1;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", id }
        };

        // Reuse ExecuteQueryAsync but just map one row
        var results = await ExecuteQueryAsync(query, parameters, CreateIngestionSource);
        return results.FirstOrDefault(); // Return single item or null
    }

    public async Task<IngestionSource> GetIngestionSourceByUri(string uri)
    {
        // Adjust if your metadata structure for the URI is different
        const string query = @"
        SELECT Id, Name, Content, Metadata
        FROM IngestionSources
        WHERE Metadata->>'Uri' = @Uri
        LIMIT 1;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Uri", uri }
        };

        var results = await ExecuteQueryAsync(query, parameters, CreateIngestionSource);
        return results.FirstOrDefault();
    }

    public async Task SaveIngestionSource(IngestionSource ingestionSource)
    {
        // We use an UPSERT (on conflict by primary key Id)
        string upsertQuery = @"
        INSERT INTO IngestionSources (Id, Name, Content, Metadata)
        VALUES (@Id, @Name, @Content, @Metadata)
        ON CONFLICT (Id) DO UPDATE 
            SET Name = EXCLUDED.Name,
                Content = EXCLUDED.Content,
                Metadata = EXCLUDED.Metadata;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", ingestionSource.Id },
            { "@Name", Path.GetFileName(ingestionSource.Name) },
            { "@Content", ingestionSource.Content }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Metadata", JsonSerializer.Serialize(ingestionSource.MetaData) }
        };

        await ExecuteQueryAsync(upsertQuery, parameters, jsonBParameters);
    }

    public async Task<IEnumerable<RetrievedDocument>> RetrieveData(string embeddingsTableName, PostgreSqlQueryParameters queryParameters)
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

        if (!queryParameters.MetaDataFilterIncludeAll.IsNullOrEmpty())
            whereClauses.AddRange(queryParameters.MetaDataFilterIncludeAll);

        if (!queryParameters.MetaDataFilterIncludeAny.IsNullOrEmpty())
            whereClauses.AddRange(queryParameters.MetaDataFilterIncludeAny);

        if (!queryParameters.MetaDataFilterExcludeAll.IsNullOrEmpty())
            whereClauses.Add($"NOT ({string.Join(" OR ", queryParameters.MetaDataFilterExcludeAll)})");

        if (!queryParameters.MetaDataFilterExcludeAny.IsNullOrEmpty())
            whereClauses.Add($"NOT ({string.Join(" OR ", queryParameters.MetaDataFilterExcludeAny)})");

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
            return Enumerable.Empty<RetrievedDocument>();

        return queryResult.Select(qr => new RetrievedDocument(embeddingsTableName, qr));
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
            EmbeddingContent = reader.GetString(reader.GetOrdinal("embeddingContent")),
            StartIndex = reader.GetInt64(reader.GetOrdinal("startIndex")),
            EndIndex = reader.GetInt64(reader.GetOrdinal("endIndex")),
            EmbeddingVector = null //Not relevant during retrieval
        };
    }

    private static IngestionSource CreateIngestionSource(NpgsqlDataReader reader)
    {
        // Id is never null because it's the primary key
        var id = reader.GetGuid(reader.GetOrdinal("Id"));

        var name = reader.GetString(reader.GetOrdinal("Name"));

        // For the text columns, check for DB null
        string? content = reader.IsDBNull(reader.GetOrdinal("Content"))
            ? null
            : reader.GetString(reader.GetOrdinal("Content"));

        // For JSONB, read as string and then deserialize
        string? metadataJson = reader.IsDBNull(reader.GetOrdinal("Metadata"))
            ? null
            : reader.GetString(reader.GetOrdinal("Metadata"));

        var metaData = string.IsNullOrEmpty(metadataJson)
            ? null
            : JsonSerializer.Deserialize<EmbeddingMetaData>(metadataJson);

        return new IngestionSource
        {
            Id = id,
            Name = name,
            Content = content,
            MetaData = metaData
        };
    }
}
