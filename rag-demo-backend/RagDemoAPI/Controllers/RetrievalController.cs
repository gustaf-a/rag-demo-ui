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
    [HttpPost("search")]
    public async Task<IActionResult> PerformSearch([FromBody] SearchRequest searchRequest)
    {
        var searchResults = await _retrievalHandler.DoSearch(searchRequest);
        if (searchResults.IsNullOrEmpty())
        {
            return NotFound();
        }

        return Ok(searchResults);
    }
}
