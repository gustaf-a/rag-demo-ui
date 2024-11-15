using Microsoft.AspNetCore.Mvc;
using RagDemoAPI.Generation;
using RagDemoAPI.Models;

namespace RagDemoAPI.Controllers;

/// <summary>
/// A controller which handles chat requests, for example a simple chat request or a data driven RAG chat request
/// </summary>
[ApiController]
[Route("generation")]
public class GenerationController(ILogger<GenerationController> _logger, IConfiguration configuration, IGenerationHandler _chatHandler) : ControllerBase
{
    [HttpPost("get-chat-response")]
    public async Task<IActionResult> GetChatResponse([FromBody] ChatRequest chatRequest)
    {
        var response = await _chatHandler.GetChatResponse(chatRequest);

        return Ok(response);
    }

    [HttpPost("get-rag-response")]
    public async Task<IActionResult> GetChatResponseWithRag([FromBody] ChatRequest chatRequest)
    {
        var response = await _chatHandler.GetRetrievalAugmentedChatResponse(chatRequest);

        return Ok(response);
    }
}
