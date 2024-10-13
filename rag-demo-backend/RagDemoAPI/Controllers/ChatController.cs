using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using RagDemoAPI.Models;
using ChatMessage = RagDemoAPI.Models.ChatMessage;

namespace RagDemoAPI.Controllers;

[ApiController]
[Route("Chat")]
public class ChatController(ILogger<ChatController> logger, IConfiguration configuration, Kernel kernel) : ControllerBase
{
    private readonly Kernel _kernel = kernel;

    [HttpPost("chatCompletion")]
    public async Task<string> Get([FromBody] IEnumerable<ChatMessage> chatMessages)
    {
        var prompt = new ChatPrompt(chatMessages);

        var promptString = prompt.ToFormattedString();

        var reply = await _kernel.InvokePromptAsync(promptString);

        return reply?.ToString() ?? "No reply";
    }
}
