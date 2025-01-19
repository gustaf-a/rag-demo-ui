using Shared.Models;

namespace Shared.Repositories;

public interface IRagRepository
{
    Task<IEnumerable<string>> GetTableNames();
    Task<bool> DoesTableExist(string tableName);
    Task DeleteTable(string tableName);
    Task ResetTable(DatabaseOptions databaseOptions);
    Task CreateEmbeddingsTable(DatabaseOptions databaseOptions);

    Task<IEnumerable<string>> GetUniqueMetaDataTagKeys(string tableName);
    Task<IEnumerable<string>> GetUniqueMetaDataTagValues(string tableName, string tag);

    Task InsertData(string tableName, ContentChunk content, float[] embedding, EmbeddingMetaData metaData);
    Task<IEnumerable<RetrievedDocument>> RetrieveData(string embeddingsTableName, PostgreSqlQueryParameters queryParameters);
    Task<IEnumerable<ContentChunk>> GetContentChunks(ContentChunkRequest contentChunkRequest);
}