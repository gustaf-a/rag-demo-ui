
using Shared.Models;

namespace ProcessDemo.Processes;

public interface IProcessHandler
{
    Task<ProcessInfo> GetProcess(string role, Guid guid);
    Task<IEnumerable<ProcessInfo>> GetProcesses(string role);
    Task<ProcessInfo> CreateProcessAsync(ProcessInfo processInfo);
    Task<ProcessInfo> UpdateProcessAsync(string role, Guid guid, ProcessInfo processInfo);
    Task<bool> DeleteProcessAsync(string role, Guid guid);

    Task<ProcessStepInfo> GetProcessStepAsync(Guid stepId);
    Task<ProcessStepInfo> CreateProcessStepAsync(ProcessStepInfo step);
    Task<ProcessStepInfo> UpdateProcessStepAsync(Guid stepId, ProcessStepInfo processStepInfo);
    Task<bool> DeleteProcessStepAsync(Guid stepId);

    Task<ProcessInstance> CreateProcessInstanceAsync(ProcessInstance processInstance);
    Task<ProcessInstance> StartProcessExecution(StartProcessRequest startProcessRequest);
    Task<IEnumerable<ProcessInstance>> GetProcessInstances(string role);
    Task<ProcessInstance> GetProcessInstance(Guid instanceId);
    Task<ProcessInstance> UpdateProcessInstanceAsync(Guid instanceId, ProcessInstance processInstance);
    Task<bool> DeleteProcessInstanceAsync(Guid instanceId);

    Task<ProcessStepInstance> GetProcessStepInstanceAsync(Guid stepInstanceId);
    Task<ProcessStepInstance> CreateProcessStepInstanceAsync(ProcessStepInstance stepInstance);
    Task<ProcessStepInstance> UpdateProcessStepInstanceAsync(Guid stepInstanceId, ProcessStepInstance stepInstance);
    Task<bool> DeleteProcessStepInstanceAsync(Guid stepInstanceId);
}