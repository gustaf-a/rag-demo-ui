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
    /// #  Examples
    /// 
    /// ## Simple chat request:
    /// ```   
    /// {  
    ///    "chatMessages": [
    ///    {  
    ///        "role": "user",  
    ///        "content": "What do you think about RAG systems for LLMs? Keep it short."  
    ///    }  
    ///    ]
    /// }  
    /// ```
    /// 
    /// ## Simple chat request with chat options high temperature for more creative answers:
    /// ```   
    /// {  
    ///    "chatMessages": [
    ///    {  
    ///        "role": "user",  
    ///        "content": "What do you think about RAG systems for LLMs? Keep it short."  
    ///    }  
    ///    ],
    ///    "chatOptions": {  
    ///        "temperature": 0.9
    ///    }
    /// }  
    /// ```
    /// 
    /// ## RAG chat request:
    /// ```   
    /// {  
    ///    "chatMessages": [
    ///    {  
    ///        "role": "user",  
    ///        "content": "How long until Jane's next meeting?"  
    ///    }  
    ///    ],
    ///    "searchOptions": {
    ///        "embeddingsTableName": "embeddings1",
    ///        "itemsToRetrieve": 3
    ///    }  
    /// }  
    /// ```
    /// 
    /// ## RAG chat request with search filters:
    /// ```   
    /// {  
    ///    "chatMessages": [
    ///    {  
    ///        "role": "user",  
    ///        "content": "How long until Jane's next meeting?"  
    ///    }  
    ///    ],
    ///    "searchOptions": {
    ///        "embeddingsTableName": "embeddings1",
    ///        "itemsToRetrieve": 3
    ///    }  
    /// }  
    /// ```
    /// 
    /// ## RAG chat request with 1 plugin:
    /// ```   
    /// {  
    ///    "chatMessages": [
    ///    {  
    ///        "role": "user",  
    ///        "content": "How long until Jane's next meeting?"  
    ///    }  
    ///    ],  
    ///    "chatOptions": {  
    ///        "temperature": 0.2,  
    ///        "pluginsToUse": [
    ///            "DatePlugin"
    ///        ]  
    ///    },  
    ///    "searchOptions": {
    ///        "embeddingsTableName": "embeddings1",
    ///        "itemsToRetrieve": 3
    ///    }  
    /// }
    /// ```
    /// 
    /// ## RAG chat request with 1 plugin without auto invoking of plugins. For human-in-the-loop or animating message progress
    /// ```
    /// {
    ///     "chatMessages": [
    ///     {
    ///         "role": "user",
    ///         "content": "How many days until Jane's next follow up?"
    ///     }
    ///     ],
    ///     "searchOptions": {
    ///         "embeddingsTableName": "embeddings1",
    ///         "itemsToRetrieve": 2,
    ///         "itemsToSkip": 0
    ///     },
    ///     "chatOptions": {
    ///         "temperature": 0.2,
    ///         "pluginsToUse": [
    ///             "DatePlugin"
    ///         ],
    ///	    "PluginsAutoInvoke":false
    ///     }
    /// }
    /// ```
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
    /// 
    /// //TODO Add example
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
