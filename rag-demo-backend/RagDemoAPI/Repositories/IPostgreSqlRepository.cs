using RagDemoAPI.Models;

namespace RagDemoAPI.Repositories;

public interface IPostgreSqlRepository
{
    Task<IEnumerable<string>> GetTableNames();
    Task<bool> DoesTableExist(DatabaseOptions databaseOptions);
    Task ResetTable(DatabaseOptions databaseOptions);
    Task SetupTable(DatabaseOptions databaseOptions);

    Task<IEnumerable<string>> GetUniqueMetaDataTagValues(DatabaseOptions databaseOptions, string tag);

    Task InsertData(DatabaseOptions databaseOptions, string content, float[] embedding, EmbeddingMetaData metaData);
    Task<IEnumerable<RetrievedDocument>> RetrieveData(DatabaseOptions databaseOptions, PostgreSqlQueryParameters queryParameters);
    Task<IEnumerable<string>> GetUniqueMetaDataTagKeys(DatabaseOptions databaseOptions);
}