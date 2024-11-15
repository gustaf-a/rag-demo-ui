using Microsoft.AspNetCore.Mvc;
using RagDemoAPI.Configuration;
using RagDemoAPI.Models;
using RagDemoAPI.Repositories;

namespace RagDemoAPI.Controllers;

[ApiController]
[Route("Database")]
public class DatabaseController(ILogger<IngestionController> _logger, IConfiguration configuration, IPostgreSqlRepository _postgreSqlService) : ControllerBase
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    [HttpGet("get-tables")]
    public async Task<IActionResult> GetDatabaseTables()
    {
        try
        {
            var tableNames = await _postgreSqlService.GetTableNames();
            return Ok(tableNames);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting names of tables: {ex.Message}");
        }
    }

    [HttpPost("reset-table")]
    public async Task<IActionResult> ResetTable([FromBody] DatabaseOptions databaseOptions)
    {
        try
        {
            await _postgreSqlService.ResetTable(databaseOptions);
            return Ok("Database reset successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error setting up tables: {ex.Message}");
        }
    }

    [HttpPost("setup-table")]
    public async Task<IActionResult> SetupTable([FromBody] DatabaseOptions databaseOptions)
    {
        try
        {
            await _postgreSqlService.SetupTable(databaseOptions);
            return Ok($"Table {databaseOptions.TableName} set up successfully with {databaseOptions.EmbeddingsDimensions} embedding dimensions.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error setting up table: {ex.Message}");
        }
    }
}
