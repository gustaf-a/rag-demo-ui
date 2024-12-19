using Shared.Models;

namespace ProcessDemo.Steps.StepImplementations;

public interface IProcessStep
{
    string StepClassName { get; }
    Task<ProcessStepExecutionResult> Execute(ProcessStepInstance processInstanceStepId);
}
