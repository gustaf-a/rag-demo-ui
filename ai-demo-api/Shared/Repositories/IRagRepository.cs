using Shared.Models;
using Shared.Models;

namespace Shared.Repositories;

public interface IRagRepository
{
    Task<IEnumerable<string>> GetTableNames();
    Task<bool> DoesTableExist(DatabaseOptions databaseOptions);
    Task ResetTable(DatabaseOptions databaseOptions);
    Task DeleteTable(DatabaseOptions databaseOptions);
    Task CreateEmbeddingsTable(DatabaseOptions databaseOptions);

    Task<IEnumerable<string>> GetUniqueMetaDataTagValues(DatabaseOptions databaseOptions, string tag);

    Task InsertData(DatabaseOptions databaseOptions, ContentChunk content, float[] embedding, EmbeddingMetaData metaData);
    Task<IEnumerable<RetrievedDocument>> RetrieveData(string embeddingsTableName, PostgreSqlQueryParameters queryParameters);
    Task<IEnumerable<string>> GetUniqueMetaDataTagKeys(DatabaseOptions databaseOptions);
    Task<IEnumerable<ContentChunk>> GetContentChunks(ContentChunkRequest contentChunkRequest);
}