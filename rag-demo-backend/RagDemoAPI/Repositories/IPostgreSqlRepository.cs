using RagDemoAPI.Models;

namespace RagDemoAPI.Repositories;

public interface IPostgreSqlRepository
{
    Task<IEnumerable<string>> GetTableNames();
    Task<bool> DoesTableExist(DatabaseOptions databaseOptions);
    Task ResetTable(DatabaseOptions databaseOptions);
    Task SetupTable(DatabaseOptions databaseOptions);

    Task InsertData(DatabaseOptions databaseOptions, string content, float[] embedding, EmbeddingMetaData metaData);
    Task<IEnumerable<RetrievedDocument>> RetrieveData(DatabaseOptions databaseOptions, PostgreSqlQueryParameters queryParameters);
}