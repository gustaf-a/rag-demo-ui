namespace Shared.Models;

public class IngestDataRequest
{
    public string? FolderPath { get; set; }

    public Dictionary<string, string>? MetaDataTags { get; set; } = [];

    public DatabaseOptions DatabaseOptions { get; set; }

    public IngestFromAzureContainerOptions? IngestFromAzureContainerOptions { get; set; }

    public IngestDataOptions? IngestDataOptions { get; set; } = new();
}
