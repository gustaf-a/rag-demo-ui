using Shared.Models;

namespace Shared.Repositories;

public interface IWorkflowRepository
{
    Task<bool> DeleteWorkflowAsync(Guid workflowId);
    Task<WorkflowInfo> GetWorkflowAsync(Guid workflowId);
    Task<IEnumerable<WorkflowInfo>> GetAllWorkflowsAsync();
    Task<IEnumerable<WorkflowInstanceInfo>> GetAllWorkflowInstancesAsync();
    Task<WorkflowInfo> SaveWorkflowAsync(WorkflowInfo workflow);
    Task<bool> UpdateWorkflowAsync(WorkflowInfo workflow);
    
    Task<bool> DeleteWorkflowStepAsync(Guid workflowStepId);
    Task<List<WorkflowStepInfo>> GetWorkflowStepsAsync(Guid workflowId);
    Task SaveWorkflowStepAsync(WorkflowStepInfo step);
    Task<bool> UpdateWorkflowStepAsync(WorkflowStepInfo step);

    Task<bool> DeleteWorkflowInstanceAsync(Guid workflowInstanceId);
    Task<WorkflowInstanceInfo> GetWorkflowInstanceAsync(Guid workflowInstanceId);
    Task<WorkflowInstanceInfo> StartWorkflowInstanceAsync(Guid workflowId, string startedBy);
    Task<bool> UpdateWorkflowInstanceStatusAsync(Guid workflowInstanceId, string status);
    
    Task<bool> DeleteWorkflowStepInstanceAsync(Guid stepInstanceId);
    Task<WorkflowStepInstanceInfo> GetWorkflowStepInstanceAsync(Guid stepInstanceId);
    Task SaveWorkflowStepInstanceAsync(WorkflowStepInstanceInfo stepInstance);
    Task<bool> UpdateWorkflowStepInstanceStatusAsync(Guid stepInstanceId, string status);
}