namespace Shared.Models;

public class ProcessStepInstance
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string StepClassName { get; set; }
    public Guid ProcessInstanceId { get; set; }
    public Guid ProcessStepId { get; set; }
    public string Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ProcessPayload Payload { get; set; }
}
