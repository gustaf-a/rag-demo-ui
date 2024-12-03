using Microsoft.AspNetCore.Mvc;
using AiDemos.Api.Extensions;
using AiDemos.Api.Models;
using AiDemos.Api.Retrieval;

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
    ///         "embeddingsTableName": "embeddings1",
    ///         "itemsToRetrieve": 3,
    ///         "semanticSearchContent": "John's medicines"
    ///     }
    /// }
    /// ```
    /// 
    /// ## Semantic search request with metadata filter (uses Tags):
    /// ```   
    /// {
    ///   "searchOptions": {
    ///     "embeddingsTableName": "embeddings1",
    ///     "itemsToRetrieve": 5,
    ///     "semanticSearchContent": "John's medicines",
    ///     "metaDataInclude": {
    ///       "project": [
    ///         "Health Care 10x"
    ///       ]
    ///     }
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="searchRequest"></param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<IActionResult> PerformSearch([FromBody] SearchRequest searchRequest)
    {
        var searchResults = await _retrievalHandler.DoSearch(searchRequest);
        if (searchResults.IsNullOrEmpty())
        {
            return NoContent();
        }

        return Ok(searchResults);
    }
}
