using Shared.Models;

namespace ProcessDemo.Steps;

public class ProcessStepExecutionResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
    public ProcessPayload Payload { get; set; }
}