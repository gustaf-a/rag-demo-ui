using Microsoft.Extensions.Logging;
using Shared.Models;

namespace ProcessDemo.Steps.StepImplementations;

public abstract class ProcessStepBase(ILogger<ProcessStepBase> _logger)
{
    protected abstract Task<ProcessStepExecutionResult> ExecuteInternal(ProcessStepInstance stepInstance);

    public async Task<ProcessStepExecutionResult> Execute(ProcessStepInstance stepInstance)
    {
        if (!ValidateExecutionRequest(stepInstance, out var failedResult))
            return failedResult;

        return await ExecuteInternal(stepInstance);
    }

    private static bool ValidateExecutionRequest(ProcessStepInstance stepInstance, out ProcessStepExecutionResult failedResult)
    {
        failedResult = new ProcessStepExecutionResult
        {
            IsSuccess = false
        };

        if (stepInstance == null)
        {
            failedResult.Message = "Step intance was null.";
            return false;
        }

        if (stepInstance.Payload is null)
        {
            failedResult.Message = "Step payload cannot be null.";
            return false;
        }

        return true;
    }
}
