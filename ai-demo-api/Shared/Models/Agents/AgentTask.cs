using Shared.Models.Agents;

namespace Shared.Models;

public class AgentTask
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string TaskPrompt { get; set; }
    public string Status { get; set; }
    public AgentTaskPayload Payload { get; set; } = new();

    public DateTime? CompletedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
