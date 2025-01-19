namespace Shared.Models;

public class IngestFromAzureContainerOptions
{
    public string ConnectionString { get; set; }
    public string RootContainerName { get; set; }

    /// <summary>
    /// For example: version_1/business_planning/
    /// </summary>
    public string? SubFolderPrefix { get; set; }

    public bool IncludeSubfolders { get; set; }
}
