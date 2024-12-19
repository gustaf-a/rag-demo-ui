namespace Shared.Models;

/// <summary>
/// Process class that directly maps to database
/// </summary>
public class ProcessModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ProcessPayload Payload { get; set; } = new ProcessPayload();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
