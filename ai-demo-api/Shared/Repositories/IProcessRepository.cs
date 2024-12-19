using ProcessDemo.Processes;
using Shared.Models;

namespace Shared.Repositories;

public interface IProcessRepository
{
    Task<bool> DeleteProcessAsync(Guid processId);
    Task<ProcessInfo> GetProcessAsync(Guid processId);
    Task<IEnumerable<ProcessInfo>> GetAllProcesssAsync();
    Task<ProcessInfo> CreateProcessAsync(ProcessInfo processInfo);
    Task<bool> UpdateProcessAsync(ProcessInfo process);
    
    Task<bool> DeleteProcessStepAsync(Guid processStepId);
    Task<List<ProcessStepInfo>> GetProcessStepsByProcessAsync(Guid processId);
    Task<ProcessStepInfo> GetProcessStepAsync(Guid processStepId);
    Task CreateProcessStepAsync(ProcessStepInfo step);
    Task<bool> UpdateProcessStepAsync(ProcessStepInfo step);

    Task<ProcessInstance> CreateProcessInstanceAsync(ProcessInstance processInstance);
    Task<bool> DeleteProcessInstanceAsync(Guid processInstanceId);
    Task<ProcessInstance> GetProcessInstanceAsync(Guid processInstanceId);
    Task<IEnumerable<ProcessInstance>> GetAllProcessInstancesAsync();
    Task<bool> UpdateProcessInstanceStatusAsync(Guid processInstanceId, string status);
    Task<bool> UpdateProcessInstanceAsync(ProcessInstance processInstance);

    Task<bool> DeleteProcessStepInstanceAsync(Guid stepInstanceId);
    Task<ProcessStepInstance> GetProcessStepInstanceAsync(Guid stepInstanceId);
    Task<ProcessStepInstance> CreateProcessStepInstanceAsync(ProcessStepInstance stepInstance);
    Task<bool> UpdateProcessStepInstanceAsync(ProcessStepInstance result);
    Task<bool> UpdateProcessStepInstanceStatusAsync(Guid stepInstanceId, string status);
    Task<IEnumerable<ProcessInfo>> GetAllProcessesAsync();
}