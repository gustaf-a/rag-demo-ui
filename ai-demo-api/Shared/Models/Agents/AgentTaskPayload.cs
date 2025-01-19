namespace Shared.Models.Agents;

public class AgentTaskPayload
{
    public Dictionary<string, object> State { get; set; } = [];
    public List<Guid> Agents { get; set; } = [];
    public List<ChatMessage> ChatHistory { get; set; } = [];
}
