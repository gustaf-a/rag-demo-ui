namespace Shared.Models;

/// <summary>
/// Simplified Process class used for sending to frontend
/// </summary>
public class ProcessInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ProcessStepInfo> Steps { get; set; } = [];
}
