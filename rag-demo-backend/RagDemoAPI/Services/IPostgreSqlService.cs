using RagDemoAPI.Models;

namespace RagDemoAPI.Services;

public interface IPostgreSqlService
{
    Task ResetDatabase();
    Task SetupTables();
    Task InsertData(string content, float[] embedding, EmbeddingMetaData metaData);
    Task<IEnumerable<RetrievedDocument>> RetrieveData(PostgreSqlQueryParameters queryParameters);
}