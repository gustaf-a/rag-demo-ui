namespace RagDemoAPI.Models;

public class IngestDataRequest
{
    public string FolderPath { get; set; }

    public IngestDataOptions IngestDataOptions { get; set; } = new();
}
