using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Repositories;
using Shared.Configuration;

namespace AiDemos.Api.Controllers.RagDemo;

[ApiController]
[Route("Database")]
public class DatabaseController(ILogger<IngestionController> _logger, IConfiguration configuration, IRagRepository _postgreSqlService) : ControllerBase
{
    private readonly AzureOptions _azureOptions = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>() ?? throw new ArgumentNullException(nameof(AzureOptions));

    [HttpGet("get-tables")]
    public async Task<ActionResult<IEnumerable<string>>> GetDatabaseTables()
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

    [HttpDelete("remove-table")]
    public async Task<ActionResult<string>> RemoveTable([FromBody] DatabaseOptions databaseOptions)
    {
        await CheckTableExists(databaseOptions);

        try
        {
            await _postgreSqlService.DeleteTable(databaseOptions);
            return Ok("Table removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to remove table: {databaseOptions.TableName}");

            return StatusCode(500, $"Failed to remove table {databaseOptions.TableName}: {ex.Message}");
        }
    }

    [HttpPost("reset-table")]
    public async Task<ActionResult<string>> ResetTable([FromBody] DatabaseOptions databaseOptions)
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

    [HttpPost("create-embeddings-table")]
    public async Task<ActionResult<string>> CreateEmbeddingsTable([FromBody] DatabaseOptions databaseOptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.TableName);

        databaseOptions.TableName = databaseOptions.TableName.ToLower();

        if (await _postgreSqlService.DoesTableExist(databaseOptions))
            throw new Exception($"Table {databaseOptions.TableName} already exists.");

        try
        {
            await _postgreSqlService.CreateEmbeddingsTable(databaseOptions);
            return Ok($"Table {databaseOptions.TableName} set up successfully with {databaseOptions.EmbeddingsDimensions} embedding dimensions.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create embeddings table: {databaseOptions.TableName}");

            return StatusCode(500, $"Failed to create embeddings table: {ex.Message}");
        }
    }

    [HttpPost("get-unique-tag-values/{tag}")]
    public async Task<ActionResult<IEnumerable<string>>> GetUniqueMetaDataTagValues(string tag, [FromBody] DatabaseOptions databaseOptions)
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
    public async Task<ActionResult<IEnumerable<string>>> GetUniqueMetaDataTagKeys([FromBody] DatabaseOptions databaseOptions)
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

        databaseOptions.TableName = databaseOptions.TableName.ToLower();

        if (!await _postgreSqlService.DoesTableExist(databaseOptions))
            throw new Exception($"Table {databaseOptions.TableName} not found.");
    }
}
