using RagDemoAPI.Configuration;
using RagDemoAPI.Models;

namespace RagDemoAPI.Retrieval.Search;

public class SearchServicePostgreSql(IConfiguration configuration) : ISearchService
{
    private readonly PostgreSqlOptions _postgreSqlOptions = configuration.GetSection(PostgreSqlOptions.PostgreSql).Get<PostgreSqlOptions>() ?? throw new ArgumentNullException(nameof(PostgreSqlOptions));

    public Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(ChatRequest chatRequest)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<RetrievedDocument>> RetrieveDocuments(SearchRequest searchRequest)
    {
        throw new NotImplementedException();
    }
}
