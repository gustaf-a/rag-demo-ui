using Microsoft.AspNetCore.Mvc;
using RagDemoAPI.Extensions;
using RagDemoAPI.Models;
using RagDemoAPI.Retrieval;

namespace RagDemoAPI.Controllers;

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
