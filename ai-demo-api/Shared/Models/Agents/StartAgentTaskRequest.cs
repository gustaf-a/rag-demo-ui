namespace Shared.Models.Agents;

public class StartAgentTaskRequest
{
    public string TaskPrompt { get; set; }
    public string? Name { get; set; }

    /// <summary>
    /// List of agents either by ID or name
    /// </summary>
    public List<string> Agents { get; set; }
    
    public TerminationStrategyInfo? TerminationStrategyInfo { get; set; }
    public Dictionary<string, string> Options { get; set; } = [];
}
