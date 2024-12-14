namespace Shared.Models;

public class ProcessStepInfo
{
    public Guid Id { get; set; }
    public Guid ProcessId { get; set; }
    public string Name { get; set; }
    public string StepClassName { get; set; }
    public string Status { get; set; } = ProcessStatus.NotStarted;
    public DateTime? StatusChanged { get; set; }
    public IDictionary<string, string> Properties { get; set; }

    /// <summary>
    /// Steps that must be completed before this step can start (Fan-In).
    /// </summary>
    public List<Guid> PredecessorStepIds { get; set; }

    /// <summary>
    /// Steps that depend on the completion of this step (Fan-Out).
    /// </summary>
    public List<Guid> SuccessorStepIds { get; set; }

    /// <summary>
    /// Optional payload or data associated with the step.
    /// </summary>
    public ProcessPayload Payload { get; set; }
}
