namespace Shared.Models;

public class AgentTaskModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string TaskPrompt { get; set; }
    public string Status { get; set; }
    public string Payload { get; set; }

    public DateTime? CompletedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
