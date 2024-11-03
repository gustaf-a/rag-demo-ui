using Microsoft.AspNetCore.Mvc;
using RagDemoAPI.Configuration;
using RagDemoAPI.Ingestion;
using RagDemoAPI.Models;
using RagDemoAPI.Services;

namespace RagDemoAPI.Controllers;

[ApiController]
[Route("Ingestion")]
public class IngestionController(ILogger<IngestionController> _logger, IConfiguration configuration, IPostgreSqlService _postgreSqlService, IIngestionHandler _ingestionHandler) : ControllerBase
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    [HttpGet("chunking-handlers")]
    public async Task<IActionResult> GetChunkingHandlers()
    {
        var chunkingHandlerNames = _ingestionHandler.GetChunkingHandlerNames();

        return Ok(chunkingHandlerNames);
    }

    [HttpPost("reset-database")]
    public async Task<IActionResult> ResetDatabase()
    {
        try
        {
            await _postgreSqlService.ResetDatabaseAsync();
            return Ok("Database reset successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error setting up tables: {ex.Message}");
        }
    }

    [HttpPost("setup-tables")]
    public async Task<IActionResult> SetupTables()
    {
        try
        {
            await _postgreSqlService.SetupTablesAsync();
            return Ok("Tables set up successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error setting up tables: {ex.Message}");
        }
    }

    [HttpPost("ingest-data")]
    public async Task<IActionResult> IngestData([FromBody] IngestDataRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request?.FolderPath);

        try
        {
            await _ingestionHandler.IngestDataFromFolderAsync(request);

            return Ok("Data ingested successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error ingesting data: {ex.Message}");
        }
    }
}
