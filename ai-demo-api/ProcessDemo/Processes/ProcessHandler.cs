using ProcessDemo.Processes;
using Shared.Models;
using Shared.Repositories;

namespace ProcessDemo.Processes;

public class ProcessHandler(IProcessRepository _processRepository, IProcessExecutor _processExecuter) : IProcessHandler
{
    public async Task<ProcessInfo> GetProcess(string role, Guid guid)
    {
        // Optionally, add role-based authorization logic here
        return await _processRepository.GetProcessAsync(guid);
    }

    public async Task<ProcessStepInfo> CreateProcessStepAsync(ProcessStepInfo step)
    {
        await _processRepository.CreateProcessStepAsync(step);

        return await GetProcessStepAsync(step.Id);
    }

    public async Task<IEnumerable<ProcessInfo>> GetProcesses(string role)
    {
        // Optionally, add role-based filtering logic here
        return await _processRepository.GetAllProcessesAsync();
    }

    public async Task<ProcessInstance> GetProcessInstance(Guid instanceId)
    {
        return await _processRepository.GetProcessInstanceAsync(instanceId);
    }

    public async Task<IEnumerable<ProcessInstance>> GetProcessInstances(string role)
    {
        // Optionally, add role-based filtering logic here
        return await _processRepository.GetAllProcessInstancesAsync();
    }

    public async Task<ProcessInfo> CreateProcessAsync(ProcessInfo processInfo)
    {
        if (processInfo == null)
            throw new ArgumentNullException(nameof(processInfo));

        var createdProcess = await _processRepository.CreateProcessAsync(processInfo);

        return createdProcess;
    }

    public async Task<ProcessInfo> UpdateProcessAsync(string role, Guid guid, ProcessInfo processInfo)
    {
        if (processInfo == null)
            throw new ArgumentNullException(nameof(processInfo));

        // Optionally, add role-based authorization logic here

        if (!await _processRepository.UpdateProcessAsync(processInfo))
            return null;

        return processInfo;
    }

    public async Task<bool> DeleteProcessAsync(string role, Guid guid)
    {
        // Optionally, add role-based authorization logic here

        var existingProcess = await _processRepository.GetProcessAsync(guid);
        if (existingProcess == null)
            throw new KeyNotFoundException($"Process with ID {guid} not found.");

        var result = await _processRepository.DeleteProcessAsync(guid);
        return result;
    }

    public async Task<ProcessStepInfo> GetProcessStepAsync(Guid stepId)
    {
        return await _processRepository.GetProcessStepAsync(stepId) 
            ?? throw new KeyNotFoundException($"ProcessStep with ID {stepId} not found.");
    }

    public async Task<ProcessStepInfo> UpdateProcessStepAsync(Guid stepId, ProcessStepInfo processStepInfo)
    {
        if (processStepInfo == null)
            throw new ArgumentNullException(nameof(processStepInfo));

        var existingStep = await _processRepository.GetProcessStepAsync(stepId);
        if (existingStep == null)
            throw new KeyNotFoundException($"ProcessStep with ID {stepId} not found.");

        if (!await _processRepository.UpdateProcessStepAsync(processStepInfo))
            return null;

        return processStepInfo;
    }

    public async Task<bool> DeleteProcessStepAsync(Guid stepId)
    {
        var existingStep = await _processRepository.GetProcessStepAsync(stepId);
        if (existingStep == null)
            throw new KeyNotFoundException($"ProcessStep with ID {stepId} not found.");

        var result = await _processRepository.DeleteProcessStepAsync(stepId);
        return result;
    }

    public async Task<ProcessInstance> UpdateProcessInstanceAsync(Guid instanceId, ProcessInstance processInstance)
    {
        if (processInstance == null)
            throw new ArgumentNullException(nameof(processInstance));

        await _processRepository.UpdateProcessInstanceAsync(processInstance);

        return processInstance;
    }

    public async Task<bool> DeleteProcessInstanceAsync(Guid instanceId)
    {
        var existingInstance = await _processRepository.GetProcessInstanceAsync(instanceId);
        if (existingInstance == null)
            throw new KeyNotFoundException($"ProcessInstance with ID {instanceId} not found.");

        var result = await _processRepository.DeleteProcessInstanceAsync(instanceId);
        return result;
    }

    public async Task<ProcessStepInstance> GetProcessStepInstanceAsync(Guid stepInstanceId)
    {
        var stepInstance = await _processRepository.GetProcessStepInstanceAsync(stepInstanceId);
        if (stepInstance == null)
            throw new KeyNotFoundException($"ProcessStepInstance with ID {stepInstanceId} not found.");
        return stepInstance;
    }

    public async Task<ProcessStepInstance> CreateProcessStepInstanceAsync(ProcessStepInstance stepInstance)
    {
        if (stepInstance == null)
            throw new ArgumentNullException(nameof(stepInstance));

        var createStepInstance = await _processRepository.CreateProcessStepInstanceAsync(stepInstance);

        return createStepInstance;
    }

    public async Task<ProcessStepInstance> UpdateProcessStepInstanceAsync(Guid stepInstanceId, ProcessStepInstance stepInstance)
    {
        if (stepInstance == null)
            throw new ArgumentNullException(nameof(stepInstance));

        var existingStepInstance = await _processRepository.GetProcessStepInstanceAsync(stepInstanceId);
        if (existingStepInstance == null)
            throw new KeyNotFoundException($"ProcessStepInstance with ID {stepInstanceId} not found.");

        if (!await _processRepository.UpdateProcessStepInstanceAsync(stepInstance))
            throw new Exception($"Update failed.");

        return stepInstance;
    }

    public async Task<bool> DeleteProcessStepInstanceAsync(Guid stepInstanceId)
    {
        var existingStepInstance = await _processRepository.GetProcessStepInstanceAsync(stepInstanceId);
        if (existingStepInstance == null)
            throw new KeyNotFoundException($"ProcessStepInstance with ID {stepInstanceId} not found.");

        var result = await _processRepository.DeleteProcessStepInstanceAsync(stepInstanceId);
        return result;
    }

    public async Task<ProcessInstance> CreateProcessInstanceAsync(ProcessInstance processInstance)
    {
        var createdProcessInstance = await _processRepository.CreateProcessInstanceAsync(processInstance);

        return createdProcessInstance;
    }

    /// <summary>
    /// Starts a process by creating a process instance and executing it.
    /// </summary>
    public async Task<ProcessInstance> StartProcessExecution(StartProcessRequest startProcessRequest)
    {
        var processInstance = new ProcessInstance
        {
            Id = Guid.NewGuid(),
            Name = startProcessRequest.Name,
            Payload = startProcessRequest.Payload,
            ProcessId = startProcessRequest.ProcessId,
            Status = ProcessStatus.NotStarted,
            StartedBy = startProcessRequest.StartedBy,
            StartedAt = DateTime.UtcNow
        };

        foreach (var stepWithPayload in startProcessRequest.StepIdsWithPayload)
        {
            var payload = stepWithPayload.Value ?? new ProcessPayload();

            var stepInstance = new ProcessStepInstance
            {
                Id = Guid.NewGuid(),
                Payload = payload,
                ProcessInstanceId = processInstance.Id,
                ProcessStepId = stepWithPayload.Key,
                Status = ProcessStatus.NotStarted
            };

            processInstance.StepInstances.Add(stepInstance);
        }

        var createdProcessInstance = await _processRepository.CreateProcessInstanceAsync(processInstance);

        await _processExecuter.RunProcessInstance(createdProcessInstance);

        return createdProcessInstance;
    }

    public async Task<ProcessInstance> ResumeProcessInstanceExecution(Guid processInstanceId)
    {
        var processInstance = await _processRepository.GetProcessInstanceAsync(processInstanceId);

        if (processInstance == null)
            throw new Exception($"Process instance {processInstanceId} not found. Ensure StartProcess has been run before trying to resume the process.");

        await _processExecuter.RunProcessInstance(processInstance);

        return processInstance;
    }
}
