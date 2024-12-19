using Microsoft.Extensions.Logging;
using ProcessDemo.Steps;
using Shared.Extensions;
using Shared.Models;
using Shared.Repositories;
using Shared.Search;

namespace ProcessDemo.Processes;

public class ProcessExecutor(ILogger<ProcessExecutor> _logger, IProcessRepository _processRepository, IStepClassFactory _stepClassFactory) : IProcessExecutor
{
    public async Task RunProcessInstance(ProcessInstance processInstance)
    {
        for (int i = 0; i < processInstance.StepInstances.Count; i++)
        {
            var stepInstance = processInstance.StepInstances[i];

            if (stepInstance.Status == ProcessStatus.Completed
                || stepInstance.Status == ProcessStatus.Skipped)
                continue;

            if (stepInstance.Status != ProcessStatus.NotStarted)
                throw new Exception($"Invalid status code on step instance {stepInstance.Id}: {stepInstance.Status}");

            try
            {
                MergeInPayloadFromPreviousStep(processInstance, stepInstance, i);

                await _processRepository.UpdateProcessStepInstanceStatusAsync(stepInstance.Id, ProcessStatus.InProgress);

                var stepClass = _stepClassFactory.Create(stepInstance.StepClassName);

                var result = await stepClass.Execute(stepInstance);

                stepInstance.Payload = result.Payload;
                stepInstance.Status = result.Status ?? ProcessStatus.Completed;

                await _processRepository.UpdateProcessStepInstanceAsync(stepInstance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while executing process instance {processInstance.Id} - {processInstance.Name}, step: {stepInstance.Id} - {stepInstance.Name}");

                stepInstance.Status = ProcessStatus.Failed;
                stepInstance.Payload.EndingState.Add(ProcessPayloadKeys.Error, ex);
                stepInstance.Payload.EndingState.Add(ProcessPayloadKeys.ErrorMessage, ex.Message);

                await _processRepository.UpdateProcessStepInstanceAsync(stepInstance);

                await _processRepository.UpdateProcessInstanceStatusAsync(stepInstance.ProcessStepId, ProcessStatus.Failed);
            }

            processInstance.Payload.EndingState = stepInstance.Payload.EndingState;
        }

        await _processRepository.UpdateProcessInstanceAsync(processInstance);
    }

    private static void MergeInPayloadFromPreviousStep(ProcessInstance processInstance, ProcessStepInstance stepInstance, int i)
    {
        if (i < 0 || i >= processInstance.StepInstances.Count)
            throw new ArgumentOutOfRangeException(nameof(i), "Index is out of range of the StepInstances list.");

        stepInstance.Payload ??= new ProcessPayload();
        processInstance.Payload ??= new ProcessPayload();

        stepInstance.Payload.StartingState.AddOrUpdateRange(processInstance.Payload.Options);

        if (i == 0)
        {
            //Add process instance state if first step
            stepInstance.Payload.StartingState.AddOrUpdateRange(processInstance.Payload.StartingState);
            return;
        }

        var previousStep = processInstance.StepInstances[i - 1];
        previousStep.Payload ??= new ProcessPayload();

        stepInstance.Payload.StartingState.AddOrUpdateRange(previousStep.Payload.EndingState);
    }
}
