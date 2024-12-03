using AiDemos.Api.Models;

namespace AiDemos.Api.Repositories;

public interface IPostgreSqlRepository
{
    Task<IEnumerable<string>> GetTableNames();
    Task<bool> DoesTableExist(DatabaseOptions databaseOptions);
    Task ResetTable(DatabaseOptions databaseOptions);
    Task DeleteTable(DatabaseOptions databaseOptions);
    Task CreateEmbeddingsTable(DatabaseOptions databaseOptions);

    Task<IEnumerable<string>> GetUniqueMetaDataTagValues(DatabaseOptions databaseOptions, string tag);

    Task InsertData(DatabaseOptions databaseOptions, string content, float[] embedding, EmbeddingMetaData metaData);
    Task<IEnumerable<RetrievedDocument>> RetrieveData(string embeddingsTableName, PostgreSqlQueryParameters queryParameters);
    Task<IEnumerable<string>> GetUniqueMetaDataTagKeys(DatabaseOptions databaseOptions);
}