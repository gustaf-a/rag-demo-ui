namespace Shared.Models.Agents;

public class TerminationStrategyInfo
{
    public string Type { get; set; }
    public Dictionary<string, object> Payload { get; set; }
}