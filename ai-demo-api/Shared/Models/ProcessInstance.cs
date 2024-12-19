namespace Shared.Models;

public class ProcessInstance
{
    public Guid Id { get; set; }
    public Guid ProcessId { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string StartedBy { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get;  set; }

    public List<ProcessStepInstance> StepInstances { get; set; }
    public ProcessPayload Payload { get; set; }
}
