namespace Shared.Models;

public class ProcessPayload
{
    public Dictionary<string, object> Options { get; set; } = [];
    public Dictionary<string, object> StartingState { get; set; } = [];
    public Dictionary<string, object> EndingState { get; set; } = [];
}
