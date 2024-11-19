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
            _logger.LogError(ex, $"Failed to get tables.");

            return StatusCode(500, $"Error getting names of tables: {ex.Message}");
        }
    }

    //TODO Remove table

    [HttpPost("reset-table")]
    public async Task<IActionResult> ResetTable([FromBody] DatabaseOptions databaseOptions)
    {
        await CheckTableExists(databaseOptions);

        try
        {
            await _postgreSqlService.ResetTable(databaseOptions);
            return Ok("Database reset successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to reset table: {databaseOptions.TableName}");

            return StatusCode(500, $"Error setting up tables: {ex.Message}");
        }
    }

    [HttpPost("setup-table")]
    public async Task<IActionResult> SetupTable([FromBody] DatabaseOptions databaseOptions)
    {
        await CheckTableExists(databaseOptions);

        try
        {
            await _postgreSqlService.SetupTable(databaseOptions);
            return Ok($"Table {databaseOptions.TableName} set up successfully with {databaseOptions.EmbeddingsDimensions} embedding dimensions.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to set up table: {databaseOptions.TableName}");

            return StatusCode(500, $"Error setting up table: {ex.Message}");
        }
    }

    [HttpPost("get-unique-tag-values/{tag}")]
    public async Task<IActionResult> GetUniqueMetaDataTagValues(string tag, [FromBody]DatabaseOptions databaseOptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        await CheckTableExists(databaseOptions);

        try
        {
            var projectNames = await _postgreSqlService.GetUniqueMetaDataTagValues(databaseOptions, tag);
            return Ok(projectNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get {tag} values.");

            return StatusCode(500, $"Error getting {tag} values: {ex.Message}");
        }
    }
    
    [HttpPost("get-unique-tag-keys")]
    public async Task<IActionResult> GetUniqueMetaDataTagKeys([FromBody]DatabaseOptions databaseOptions)
    {
        await CheckTableExists(databaseOptions);

        try
        {
            var projectNames = await _postgreSqlService.GetUniqueMetaDataTagKeys(databaseOptions);
            return Ok(projectNames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get tag keys {databaseOptions.TableName}.");

            return StatusCode(500, $"Failed to get tag keys {databaseOptions.TableName}: {ex.Message}");
        }
    }

    private async Task CheckTableExists(DatabaseOptions databaseOptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.TableName);

        if (!await _postgreSqlService.DoesTableExist(databaseOptions))
            throw new Exception($"Table {databaseOptions.TableName} not found.");
    }
}
