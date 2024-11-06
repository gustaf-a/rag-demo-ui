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

    [HttpGet("chunkers")]
    public async Task<IActionResult> GetChunkers()
    {
        var chunkerNames = _ingestionHandler.GetChunkerNames();

        return Ok(chunkerNames);
    }

    [HttpPost("reset-database")]
    public async Task<IActionResult> ResetDatabase()
    {
        try
        {
            await _postgreSqlService.ResetDatabase();
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
            await _postgreSqlService.SetupTables();
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
            await _ingestionHandler.IngestDataFromFolder(request);

            return Ok("Data ingested successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error ingesting data: {ex.Message}");
        }
    }
}
