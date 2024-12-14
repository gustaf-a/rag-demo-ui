using Shared.Models;

namespace ProcessDemo.Processs;

public class StartProcessRequest
{
    public string Name { get; set; }
    public string StartedBy { get; set; }
    public Guid ProcessId { get; set; }
    public ProcessPayload Payload { get; set; }

    public Dictionary<Guid, ProcessPayload?> StepIdsWithPayload { get; set; }
}