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
    [HttpGet("chat-sources")]
    public async Task<IActionResult> GetSourcesForChatRequest([FromBody] ChatRequest chatRequest)
    {
        var retrievedContextSources = await _retrievalHandler.RetrieveContextForQuery(chatRequest);
        if (retrievedContextSources.IsNullOrEmpty())
        {
            return NotFound();
        }

        return Ok(retrievedContextSources);
    }
}
