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
    /// <summary>
    /// Gets a chat response.
    /// If SearchOptions are included, search results will be included for the model to use for generation.
    /// If ProvidedDocumentSources are included, they will be used instead of searching.
    /// </summary>
    /// <remarks>
    /// Example request:
    /// {
    ///  "embeddingsTableName": "embeddings1",
    ///  "chatMessages": [
    ///    {
    ///      "role": "user",
    ///      "content": "How long until Jane's next meeting?"
    ///    }
    ///  ],
    ///  "chatOptions": {
    ///    "temperature": 0.2,
    ///    "pluginsToUse": [
    ///      "DatePlugin"
    ///    ]
    ///  },
    ///  "searchOptions": {
    ///    "embeddingsTableName": "embeddings1",
    ///    "itemsToRetrieve": 3
    ///    }
    ///}
    /// </remarks>
    /// <param name="chatRequest"></param>
    /// <returns></returns>
    [HttpPost("get-chat-response")]
    public async Task<IActionResult> GetChatResponse([FromBody] ChatRequest chatRequest)
    {
        var response = await _chatHandler.GetChatResponse(chatRequest);

        return Ok(response);
    }

    /// <summary>
    /// Continues a chat using the chat history object.
    /// Work in progress.
    /// 
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="continueChatRequest"></param>
    /// <returns></returns>
    [HttpPost("continue-chat")]
    public async Task<IActionResult> ContinueChatResponse([FromBody] ContinueChatRequest continueChatRequest)
    {
        var response = await _chatHandler.ContinueChatResponse(continueChatRequest);

        return Ok(response);
    }
}
