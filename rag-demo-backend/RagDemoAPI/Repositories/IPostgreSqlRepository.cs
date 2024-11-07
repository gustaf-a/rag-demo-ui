using RagDemoAPI.Models;

namespace RagDemoAPI.Repositories;

public interface IPostgreSqlRepository
{
    Task ResetDatabase();
    Task SetupTables();
    Task InsertData(string content, float[] embedding, EmbeddingMetaData metaData);
    Task<IEnumerable<RetrievedDocument>> RetrieveData(PostgreSqlQueryParameters queryParameters);
}