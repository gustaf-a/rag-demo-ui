using Microsoft.AspNetCore.Mvc;
using AiDemos.Api.Configuration;
using AiDemos.Api.Ingestion;
using AiDemos.Api.Models;

namespace AiDemos.Api.Controllers.RagDemo;

[ApiController]
[Route("Ingestion")]
public class IngestionController(ILogger<IngestionController> _logger, IConfiguration configuration, IIngestionHandler _ingestionHandler) : ControllerBase
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    [HttpGet("get-chunkers")]
    public async Task<IActionResult> GetChunkers()
    {
        var chunkerNames = _ingestionHandler.GetChunkerNames();

        return Ok(chunkerNames);
    }

    [HttpPost("ingest-data")]
    public async Task<IActionResult> IngestData([FromBody] IngestDataRequest request)
    {
        ArgumentNullException.ThrowIfNull(nameof(request));

        if (string.IsNullOrWhiteSpace(request.FolderPath)
            && request.IngestFromAzureContainerOptions is null)
            throw new Exception($"Either {nameof(request.FolderPath)} or {nameof(request.IngestFromAzureContainerOptions)} must contain information.");

        try
        {
            await _ingestionHandler.IngestData(request);

            return Ok("Data ingested successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error ingesting data: {ex.Message}");
        }
    }
}
