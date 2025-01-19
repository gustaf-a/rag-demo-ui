using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using AiDemos.Api.Retrieval;
using Shared.Models;
using Shared.Extensions;

namespace AiDemos.Api.Controllers.RagDemo;

/// <summary>
/// A controller which only handles retrieval, for example for semantic search
/// </summary>
[ApiController]
[Route("retrieval")]
public class RetrievalController(ILogger<RetrievalController> _logger, IConfiguration configuration, IRetrievalHandler _retrievalHandler) : ControllerBase
{
    /// <summary>
    /// Searches a table using text and or semantic search.
    /// </summary>
    /// <remarks>
    /// #  Examples
    /// 
    /// ## Simple semantic search request:
    /// ```   
    /// {
    ///     "searchOptions": {
    ///         "embeddingsTableName": "embeddings2",
    ///         "itemsToRetrieve": 3,
    ///         "semanticSearchContent": "Nimbus installation"
    ///     }
    /// }
    /// ```
    /// 
    /// ## Simple semantic search request with contentBefore and contentAfter:
    /// ```   
    /// {
    ///     "searchOptions": {
    ///         "embeddingsTableName": "embeddings2",
    ///         "itemsToRetrieve": 2,
    ///         "semanticSearchContent": "how to talk to other departments",
    ///         "IncludeContentChunksAfter": 2,
    ///         "IncludeContentChunksBefore": 2
    ///     }
    /// }
    /// ```
    /// 
    /// ## Semantic search request with metadata filter (uses Tags):
    /// The metaDataIncludeWhenContainsAll filter indicates that the mix of the tag values must be present.
    /// The metaDataIncludeWhenContainsAny filter requires that any value provided is enough to include. However, all different tags must get a match for at least one value.
    /// 
    /// Remove any of the accessLevels in this response to see how it affects the results.
    /// 
    /// ```   
    /// {
    ///   "searchOptions": {
    ///     "embeddingsTableName": "embeddings2",
    ///     "itemsToRetrieve": 5,
    ///     "semanticSearchContent": "What happened at the summer party?",
    ///     "metaDataIncludeWhenContainsAny": {
    ///       "accessLevel": ["2","3"]
    ///     },
    ///    "IncludeContentChunksAfter": 2,
    ///    "IncludeContentChunksBefore": 2
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="searchRequest"></param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<RetrievedDocument>>> PerformSearch([FromBody] SearchRequest searchRequest)
    {
        var searchResults = await _retrievalHandler.DoSearch(searchRequest);
        if (searchResults.IsNullOrEmpty())
        {
            return NoContent();
        }

        return Ok(searchResults);
    }
    
    [HttpPost("get-chunks")]
    public async Task<ActionResult<IEnumerable<ContentChunk>>> GetChunks([FromBody] ContentChunkRequest contentChunkRequest)
    {
        var contentChunks = await _retrievalHandler.GetContentChunks(contentChunkRequest);
        if (contentChunks.IsNullOrEmpty())
        {
            return NoContent();
        }

        return Ok(contentChunks);
    }
}
