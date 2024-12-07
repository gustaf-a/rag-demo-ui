using System.Text.Json;
using AiDemos.Api.Repositories;
using Microsoft.Extensions.Configuration;
using Shared.Models;

namespace Shared.Repositories;
public class WorkflowRepository : RepositoryBase, IWorkflowRepository
{
    public WorkflowRepository(IConfiguration configuration) : base(configuration)
    {
    }

    #region Workflows

    public async Task<WorkflowInfo> SaveWorkflowAsync(WorkflowInfo workflow)
    {
        workflow.Id = Guid.NewGuid();

        var query = @"
            INSERT INTO Workflows (WorkflowId, Name, Description)
            VALUES (@WorkflowId, @Name, @Description);";

        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowId", workflow.Id },
            { "@Name", workflow.Name },
            { "@Description", workflow.Description },
        };

        await ExecuteQueryAsync(query, parameters);

        foreach (var step in workflow.Steps)
        {
            step.Id = Guid.NewGuid();
            step.WorkflowId = workflow.Id;
            await SaveWorkflowStepAsync(step);
        }

        return workflow;
    }

    public async Task<WorkflowInfo> GetWorkflowAsync(Guid workflowId)
    {
        var query = "SELECT * FROM Workflows WHERE WorkflowId = @WorkflowId;";
        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowId", workflowId }
        };

        var workflow = await ExecuteQuerySingleAsync<WorkflowInfo>(query, parameters);

        if (workflow != null)
        {
            workflow.Steps = await GetWorkflowStepsAsync(workflowId);
        }

        return workflow;
    }

    public async Task<IEnumerable<WorkflowInfo>> GetAllWorkflowsAsync()
    {
        var workflows = await ExecuteQueryAsync<WorkflowInfo>("SELECT * FROM Workflows;", []);

        if (workflows.Count == 0)
            return workflows;

        var allSteps = await ExecuteQueryAsync<WorkflowStepInfo>("SELECT * FROM WorkflowSteps;", []);

        var stepsByWorkflow = new Dictionary<Guid, List<WorkflowStepInfo>>();
        foreach (var step in allSteps)
        {
            if (!stepsByWorkflow.ContainsKey(step.WorkflowId))
                stepsByWorkflow[step.WorkflowId] = [];

            stepsByWorkflow[step.WorkflowId].Add(step);
        }

        foreach (var workflow in workflows)
        {
            if (stepsByWorkflow.TryGetValue(workflow.Id, out var steps))
                workflow.Steps = steps;
            else
                workflow.Steps = [];
        }

        return workflows;
    }


    public async Task<bool> UpdateWorkflowAsync(WorkflowInfo workflow)
    {
        var query = @"
            UPDATE Workflows
            SET Name = @Name,
                Description = @Description,
                UpdatedAt = NOW()
            WHERE WorkflowId = @WorkflowId;";

        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowId", workflow.Id },
            { "@Name", workflow.Name },
            { "@Description", workflow.Description },
        };

        await ExecuteQueryAsync(query, parameters);

        foreach (var step in workflow.Steps)
        {
            await UpdateWorkflowStepAsync(step);
        }

        return true;
    }

    public async Task<bool> DeleteWorkflowAsync(Guid workflowId)
    {
        var query = "DELETE FROM Workflows WHERE WorkflowId = @WorkflowId;";
        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowId", workflowId }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    #endregion

    #region Workflow Steps

    public async Task SaveWorkflowStepAsync(WorkflowStepInfo step)
    {
        var query = @"
            INSERT INTO WorkflowSteps (WorkflowStepId, WorkflowId, Name, PredecessorStepIds, SuccessorStepIds, Payload)
            VALUES (@WorkflowStepId, @WorkflowId, @Name, @PredecessorStepIds, @SuccessorStepIds, @Payload);";

        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowStepId", step.Id },
            { "@WorkflowId", step.WorkflowId },
            { "@Name", step.Name },
            { "@PredecessorStepIds", step.PredecessorStepIds.ToArray() },
            { "@SuccessorStepIds", step.SuccessorStepIds.ToArray() },
            { "@Payload", step.Payload != null ? JsonSerializer.Serialize(step.Payload) : null }
        };

        await ExecuteQueryAsync(query, parameters);
    }

    public async Task<List<WorkflowStepInfo>> GetWorkflowStepsAsync(Guid workflowId)
    {
        var query = "SELECT * FROM WorkflowSteps WHERE WorkflowId = @WorkflowId;";
        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowId", workflowId }
        };

        var steps = await ExecuteQueryAsync<WorkflowStepInfo>(query, parameters);
        return steps;
    }

    public async Task<bool> UpdateWorkflowStepAsync(WorkflowStepInfo step)
    {
        var query = @"
            UPDATE WorkflowSteps
            SET Name = @Name,
                PredecessorStepIds = @PredecessorStepIds,
                SuccessorStepIds = @SuccessorStepIds,
                Payload = @Payload,
                UpdatedAt = NOW()
            WHERE WorkflowStepId = @WorkflowStepId;";

        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowStepId", step.Id },
            { "@Name", step.Name },
            { "@PredecessorStepIds", step.PredecessorStepIds.ToArray() },
            { "@SuccessorStepIds", step.SuccessorStepIds.ToArray() },
            { "@Payload", step.Payload != null ? JsonSerializer.Serialize(step.Payload) : null }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    public async Task<bool> DeleteWorkflowStepAsync(Guid workflowStepId)
    {
        var query = "DELETE FROM WorkflowSteps WHERE WorkflowStepId = @WorkflowStepId;";
        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowStepId", workflowStepId }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    #endregion

    #region Workflow Instances

    public async Task<WorkflowInstanceInfo> StartWorkflowInstanceAsync(Guid workflowId, string startedBy)
    {
        var workflowInstance = new WorkflowInstanceInfo
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            Status = "InProgress",
            StartedBy = startedBy,
            StartedAt = DateTime.UtcNow
        };

        var query = @"
            INSERT INTO WorkflowInstances (WorkflowInstanceId, WorkflowId, Status, StartedBy, StartedAt)
            VALUES (@WorkflowInstanceId, @WorkflowId, @Status, @StartedBy, @StartedAt);";

        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowInstanceId", workflowInstance.Id },
            { "@WorkflowId", workflowInstance.WorkflowId },
            { "@Status", workflowInstance.Status },
            { "@StartedBy", workflowInstance.StartedBy },
            { "@StartedAt", workflowInstance.StartedAt }
        };

        await ExecuteQueryAsync(query, parameters);

        var steps = await GetWorkflowStepsAsync(workflowId);
        foreach (var step in steps)
        {
            var stepInstance = new WorkflowStepInstanceInfo
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = workflowInstance.Id,
                WorkflowStepId = step.Id,
                Status = "NotStarted"
            };
            await SaveWorkflowStepInstanceAsync(stepInstance);
        }

        return workflowInstance;
    }

    public async Task<WorkflowInstanceInfo> GetWorkflowInstanceAsync(Guid workflowInstanceId)
    {
        var query = "SELECT * FROM WorkflowInstances WHERE WorkflowInstanceId = @WorkflowInstanceId;";
        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowInstanceId", workflowInstanceId }
        };

        var instance = await ExecuteQuerySingleAsync<WorkflowInstanceInfo>(query, parameters);
        return instance;
    }

    public async Task<IEnumerable<WorkflowInstanceInfo>> GetAllWorkflowInstancesAsync()
    {
        var instances = await ExecuteQueryAsync<WorkflowInstanceInfo>("SELECT * FROM WorkflowInstances;", []);

        if (instances.Count == 0)
            return instances;

        var allStepInstances = await ExecuteQueryAsync<WorkflowStepInstanceInfo>("SELECT * FROM WorkflowStepInstances;", []);

        var stepsByInstance = new Dictionary<Guid, List<WorkflowStepInstanceInfo>>();
        foreach (var stepInstance in allStepInstances)
        {
            if (!stepsByInstance.ContainsKey(stepInstance.WorkflowInstanceId))
                stepsByInstance[stepInstance.WorkflowInstanceId] = [];

            stepsByInstance[stepInstance.WorkflowInstanceId].Add(stepInstance);
        }

        foreach (var instance in instances)
        {
            if (stepsByInstance.TryGetValue(instance.Id, out var stepInstances))
            {
                instance.StepInstances = stepInstances;
            }
            else
            {
                instance.StepInstances = [];
            }
        }

        return instances;
    }

    public async Task<bool> UpdateWorkflowInstanceStatusAsync(Guid workflowInstanceId, string status)
    {
        var query = @"
            UPDATE WorkflowInstances
            SET Status = @Status,
                UpdatedAt = NOW(),
                CompletedAt = CASE WHEN @Status = 'Completed' THEN NOW() ELSE CompletedAt END
            WHERE WorkflowInstanceId = @WorkflowInstanceId;";

        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowInstanceId", workflowInstanceId },
            { "@Status", status }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    public async Task<bool> DeleteWorkflowInstanceAsync(Guid workflowInstanceId)
    {
        var query = "DELETE FROM WorkflowInstances WHERE WorkflowInstanceId = @WorkflowInstanceId;";
        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowInstanceId", workflowInstanceId }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    #endregion

    #region Workflow Step Instances

    public async Task SaveWorkflowStepInstanceAsync(WorkflowStepInstanceInfo stepInstance)
    {
        var query = @"
            INSERT INTO WorkflowStepInstances (WorkflowStepInstanceId, WorkflowInstanceId, WorkflowStepId, Status)
            VALUES (@WorkflowStepInstanceId, @WorkflowInstanceId, @WorkflowStepId, @Status);";

        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowStepInstanceId", stepInstance.Id },
            { "@WorkflowInstanceId", stepInstance.WorkflowInstanceId },
            { "@WorkflowStepId", stepInstance.WorkflowStepId },
            { "@Status", stepInstance.Status }
        };

        await ExecuteQueryAsync(query, parameters);
    }

    public async Task<WorkflowStepInstanceInfo> GetWorkflowStepInstanceAsync(Guid stepInstanceId)
    {
        var query = "SELECT * FROM WorkflowStepInstances WHERE WorkflowStepInstanceId = @WorkflowStepInstanceId;";
        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowStepInstanceId", stepInstanceId }
        };

        var stepInstance = await ExecuteQuerySingleAsync<WorkflowStepInstanceInfo>(query, parameters);
        return stepInstance;
    }

    public async Task<bool> UpdateWorkflowStepInstanceStatusAsync(Guid stepInstanceId, string status)
    {
        var query = @"
            UPDATE WorkflowStepInstances
            SET Status = @Status,
                UpdatedAt = NOW(),
                StartedAt = CASE WHEN @Status = 'InProgress' AND StartedAt IS NULL THEN NOW() ELSE StartedAt END,
                CompletedAt = CASE WHEN @Status = 'Completed' THEN NOW() ELSE CompletedAt END
            WHERE WorkflowStepInstanceId = @WorkflowStepInstanceId;";

        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowStepInstanceId", stepInstanceId },
            { "@Status", status }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    public async Task<bool> DeleteWorkflowStepInstanceAsync(Guid stepInstanceId)
    {
        var query = "DELETE FROM WorkflowStepInstances WHERE WorkflowStepInstanceId = @WorkflowStepInstanceId;";
        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowStepInstanceId", stepInstanceId }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    #endregion

    #region Additional Methods

    public async Task<List<WorkflowStepInstanceInfo>> GetExecutableStepsAsync(Guid workflowInstanceId)
    {
        var query = $@"
            SELECT ws.*
            FROM WorkflowStepInstances ws
            WHERE ws.WorkflowInstanceId = @WorkflowInstanceId
              AND ws.Status = '{WorkflowStatus.NotStarted}'
              AND (
                  SELECT array_agg(wsi.WorkflowStepId)
                  FROM WorkflowStepInstances wsi
                  WHERE wsi.WorkflowInstanceId = @WorkflowInstanceId
                    AND wsi.Status = '{WorkflowStatus.Completed}'
              ) @> (
                  SELECT COALESCE(array_agg(predecessor), ARRAY[]::uuid[])
                  FROM (
                      SELECT unnest(ws2.PredecessorStepIds) AS predecessor
                      FROM WorkflowSteps ws2
                      WHERE ws2.WorkflowStepId = ws.WorkflowStepId
                  ) AS predecessors
              );";

        var parameters = new Dictionary<string, object>
        {
            { "@WorkflowInstanceId", workflowInstanceId }
        };

        var steps = await ExecuteQueryAsync<WorkflowStepInstanceInfo>(query, parameters);
        return steps;
    }

    #endregion
}
