using Microsoft.AspNetCore.Mvc;
using RagDemoAPI.Configuration;
using RagDemoAPI.Models;

namespace RagDemoAPI.Controllers.EarlyTest;

[ApiController]
[Route("Mock")]
public class MockClientController : ControllerBase
{
    private readonly AzureOptions _azureOptions;

    public MockClientController(ILogger<MockClientController> logger, IConfiguration configuration)
    {
        _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));
    }

    [HttpPost("chatCompletion")]
    public Task<ChatResponse> CompleteChat([FromBody] IEnumerable<ChatMessage> chatMessages)
    {
        return Task.FromResult(new ChatResponse("This is a fake mock chat response."));
    }
}
