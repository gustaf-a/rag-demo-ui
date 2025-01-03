using Shared.Models;
using Shared.Services.Search;
using Shared.Extensions;
using Shared.Repositories;

namespace AiDemos.Api.Retrieval;

public class RetrievalHandler(ISearchServiceFactory _searchServiceFactory, IRagRepository _ragRepository) : IRetrievalHandler
{
    public async Task<IEnumerable<ContentChunk>> GetContentChunks(ContentChunkRequest contentChunkRequest)
    {
        ArgumentNullException.ThrowIfNull(contentChunkRequest);

        var contentChunks = await _ragRepository.GetContentChunks(contentChunkRequest);

        return contentChunks;
    }

    public async Task<IEnumerable<RetrievedDocument>> DoSearch(SearchRequest searchRequest)
    {
        ArgumentNullException.ThrowIfNull(searchRequest);
        ArgumentNullException.ThrowIfNull(searchRequest.SearchOptions);

        var searchService = _searchServiceFactory.Create(searchRequest.SearchOptions);

        var retrievedSources = (await searchService.RetrieveDocuments(searchRequest)).ToList(); ;

        for (int i = 0; i < retrievedSources.Count; i++)
        {
            if (searchRequest.SearchOptions.IncludeContentChunksBefore > 0)
            {
                var contentChunkRequest = new ContentChunkRequest
                {
                    TableName = searchRequest.SearchOptions.EmbeddingsTableName,
                    Uri = retrievedSources[i].Uri,
                    StartChunk = retrievedSources[i].MetaData.SourceChunkNumber - searchRequest.SearchOptions.IncludeContentChunksBefore,
                    EndChunk = retrievedSources[i].MetaData.SourceChunkNumber - 1,
                };

                var contentChunks = await GetContentChunks(contentChunkRequest);
                if (contentChunks.Any())
                {
                    retrievedSources[i].ContentBefore = contentChunks.GetContents();
                }
            }

            if (searchRequest.SearchOptions.IncludeContentChunksAfter > 0)
            {
                var contentChunkRequest = new ContentChunkRequest
                {
                    TableName = searchRequest.SearchOptions.EmbeddingsTableName,
                    Uri = retrievedSources[i].Uri,
                    StartChunk = retrievedSources[i].MetaData.SourceChunkNumber + 1,
                    EndChunk = retrievedSources[i].MetaData.SourceChunkNumber + searchRequest.SearchOptions.IncludeContentChunksBefore,
                };

                var contentChunks = await GetContentChunks(contentChunkRequest);
                if (contentChunks.Any())
                {
                    retrievedSources[i].ContentAfter = contentChunks.GetContents();
                }
            }
        }

        return retrievedSources;
    }

    public async Task<IEnumerable<RetrievedDocument>> RetrieveContextForQuery(ChatRequest chatRequest)
    {
        ArgumentNullException.ThrowIfNull(chatRequest);
        ArgumentNullException.ThrowIfNull(chatRequest.SearchOptions);

        var searchService = _searchServiceFactory.Create(chatRequest.SearchOptions);

        var retrievedSources = await searchService.RetrieveDocuments(chatRequest);

        return retrievedSources;
    }
}
