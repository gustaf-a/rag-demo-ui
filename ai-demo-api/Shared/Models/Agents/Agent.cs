namespace Shared.Models.Agents;

public class Agent()
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> Options { get; set; } = [];
}
