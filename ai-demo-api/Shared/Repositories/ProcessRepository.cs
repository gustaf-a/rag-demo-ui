using System.Diagnostics;
using System.Text.Json;
using AiDemos.Api.Repositories;
using Microsoft.Extensions.Configuration;
using Shared.Models;

namespace Shared.Repositories;
public class ProcessRepository(IConfiguration configuration)
    : RepositoryBase(configuration), IProcessRepository
{
    #region Processs

    public async Task<ProcessInfo> CreateProcessAsync(ProcessInfo processInfo)
    {
        processInfo.Id = Guid.NewGuid();

        var query = @"
            INSERT INTO Processes (ProcessId, Name, Description, Payload)
            VALUES (@ProcessId, @Name, @Description, @Payload);";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessId", processInfo.Id },
            { "@Name", processInfo.Name },
            { "@Description", processInfo.Description },
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(processInfo.Payload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);

        foreach (var step in processInfo.Steps)
        {
            step.Id = Guid.NewGuid();
            step.ProcessId = processInfo.Id;

            await CreateProcessStepAsync(step);
        }

        return processInfo;
    }

    public async Task<IEnumerable<ProcessInfo>> GetAllProcessesAsync()
    {
        var query = "SELECT * FROM Processes;";

        var parameters = new Dictionary<string, object>();

        var processModels = await ExecuteQueryAsync<ProcessModel>(query, parameters);

        var processInfos = new List<ProcessInfo>();

        foreach (var processModel in processModels)
        {
            var processInfo = await GetProcessInfo(processModel, withStep: true);

            processInfos.Add(processInfo);
        }

        return processInfos;
    }

    private async Task<ProcessInfo> GetProcessInfo(ProcessModel processModel, bool withStep)
    {
        var processInfo = new ProcessInfo
        {
            Id = processModel.Id,
            Description = processModel.Description,
            Name = processModel.Name,
            Payload = processModel.Payload
        };

        if(withStep)
            processInfo.Steps = await GetProcessStepsByProcessAsync(processInfo.Id);
    
        return processInfo;
    }

    public async Task<ProcessInfo> GetProcessAsync(Guid ProcessId)
    {
        var query = "SELECT * FROM Processes WHERE ProcessId = @ProcessId;";
        var parameters = new Dictionary<string, object>
        {
            { "@ProcessId", ProcessId }
        };

        var processModel = await ExecuteQuerySingleAsync<ProcessModel>(query, parameters);

        var processInfo = await GetProcessInfo(processModel, withStep: true);

        return processInfo;
    }

    public async Task<IEnumerable<ProcessInfo>> GetAllProcesssAsync()
    {
        //TODO Get processModel
        var processes = await ExecuteQueryAsync<ProcessInfo>("SELECT * FROM Processes;", []);

        if (processes.Count == 0)
            return processes;

        var allSteps = await ExecuteQueryAsync<ProcessStepInfo>("SELECT * FROM ProcessSteps;", []);

        var stepsByProcess = new Dictionary<Guid, List<ProcessStepInfo>>();
        foreach (var step in allSteps)
        {
            if (!stepsByProcess.ContainsKey(step.ProcessId))
                stepsByProcess[step.ProcessId] = [];

            stepsByProcess[step.ProcessId].Add(step);
        }

        foreach (var process in processes)
        {
            if (stepsByProcess.TryGetValue(process.Id, out var steps))
                process.Steps = steps;
            else
                process.Steps = [];
        }

        return processes;
    }

    public async Task<bool> UpdateProcessAsync(ProcessInfo process)
    {
        var query = @"
            UPDATE Processs
            SET Name = @Name,
                Description = @Description,
                UpdatedAt = NOW()
            WHERE ProcessId = @ProcessId;";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessId", process.Id },
            { "@Name", process.Name },
            { "@Description", process.Description },
        };

        await ExecuteQueryAsync(query, parameters);

        foreach (var step in process.Steps)
        {
            await UpdateProcessStepAsync(step);
        }

        return true;
    }

    public async Task<bool> DeleteProcessAsync(Guid ProcessId)
    {
        var query = "DELETE FROM Processes WHERE ProcessId = @ProcessId;";
        var parameters = new Dictionary<string, object>
        {
            { "@ProcessId", ProcessId }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    #endregion

    #region Process Steps

    public async Task CreateProcessStepAsync(ProcessStepInfo step)
    {
        var query = @"
            INSERT INTO ProcessSteps (ProcessStepId, ProcessId, Name, PredecessorStepIds, SuccessorStepIds, Payload, StepClassName)
            VALUES (@ProcessStepId, @ProcessId, @Name, @PredecessorStepIds, @SuccessorStepIds, @Payload, @StepClassName);";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessStepId", step.Id },
            { "@ProcessId", step.ProcessId },
            { "@Name", step.Name },
            { "@StepClassName", step.StepClassName },
            { "@PredecessorStepIds", step.PredecessorStepIds.ToArray() },
            { "@SuccessorStepIds", step.SuccessorStepIds.ToArray() }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(step.Payload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);
    }

    public async Task<ProcessStepInfo> GetProcessStepAsync(Guid processStepId)
    {
        var query = "SELECT * FROM ProcessSteps WHERE ProcessStepId = @ProcessStepId;";
        var parameters = new Dictionary<string, object>
        {
            { "@ProcessStepId", processStepId }
        };

        var step = await ExecuteQuerySingleAsync<ProcessStepInfo>(query, parameters);
        return step;
    }

    public async Task<List<ProcessStepInfo>> GetProcessStepsByProcessAsync(Guid processId)
    {
        var query = "SELECT * FROM ProcessSteps WHERE ProcessId = @ProcessId;";
        var parameters = new Dictionary<string, object>
        {
            { "@ProcessId", processId }
        };

        var steps = await ExecuteQueryAsync<ProcessStepInfo>(query, parameters);
        return steps;
    }

    public async Task<bool> UpdateProcessStepAsync(ProcessStepInfo step)
    {
        var query = @"
            UPDATE ProcessSteps
            SET Name = @Name,
                PredecessorStepIds = @PredecessorStepIds,
                SuccessorStepIds = @SuccessorStepIds,
                Payload = @Payload,
                UpdatedAt = NOW()
            WHERE ProcessStepId = @ProcessStepId;";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessStepId", step.Id },
            { "@Name", step.Name },
            { "@PredecessorStepIds", step.PredecessorStepIds.ToArray() },
            { "@SuccessorStepIds", step.SuccessorStepIds.ToArray() }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(step.Payload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);

        return true;
    }

    public async Task<bool> DeleteProcessStepAsync(Guid ProcessStepId)
    {
        var query = "DELETE FROM ProcessSteps WHERE ProcessStepId = @ProcessStepId;";
        var parameters = new Dictionary<string, object>
        {
            { "@ProcessStepId", ProcessStepId }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    #endregion

    #region Process Instances

    public async Task<ProcessInstance> CreateProcessInstanceAsync(ProcessInstance processInstance)
    {
        processInstance.Id = new Guid();

        var query = @"
            INSERT INTO ProcessInstances (ProcessInstanceId, ProcessId, Status, StartedBy, StartedAt, Payload, Name)
            VALUES (@ProcessInstanceId, @ProcessId, @Status, @StartedBy, @StartedAt, @Payload, @Name);";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessInstanceId", processInstance.Id },
            { "@Name", processInstance.Name },
            { "@ProcessId", processInstance.ProcessId },
            { "@Status", processInstance.Status },
            { "@StartedBy", processInstance.StartedBy },
            { "@StartedAt", processInstance.StartedAt }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(processInstance.Payload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);

        foreach (var stepInstance in processInstance.StepInstances)
        {
            stepInstance.Id = Guid.NewGuid();

            await CreateProcessStepInstanceAsync(stepInstance);
        }

        return processInstance;
    }

    public async Task<ProcessInstance> GetProcessInstanceAsync(Guid ProcessInstanceId)
    {
        var query = "SELECT * FROM ProcessInstances WHERE ProcessInstanceId = @ProcessInstanceId;";
        var parameters = new Dictionary<string, object>
        {
            { "@ProcessInstanceId", ProcessInstanceId }
        };

        var instance = await ExecuteQuerySingleAsync<ProcessInstance>(query, parameters);
        return instance;
    }

    public async Task<IEnumerable<ProcessInstance>> GetAllProcessInstancesAsync()
    {
        var instances = await ExecuteQueryAsync<ProcessInstance>("SELECT * FROM ProcessInstances;", []);

        if (instances.Count == 0)
            return instances;

        var allStepInstances = await ExecuteQueryAsync<ProcessStepInstance>("SELECT * FROM ProcessStepInstances;", []);

        var stepsByInstance = new Dictionary<Guid, List<ProcessStepInstance>>();
        foreach (var stepInstance in allStepInstances)
        {
            if (!stepsByInstance.ContainsKey(stepInstance.ProcessInstanceId))
                stepsByInstance[stepInstance.ProcessInstanceId] = [];

            stepsByInstance[stepInstance.ProcessInstanceId].Add(stepInstance);
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

    public async Task<bool> UpdateProcessInstanceStatusAsync(Guid ProcessInstanceId, string status)
    {
        var query = @"
            UPDATE ProcessInstances
            SET Status = @Status,
                UpdatedAt = NOW(),
                CompletedAt = CASE WHEN @Status = 'Completed' THEN NOW() ELSE CompletedAt END
            WHERE ProcessInstanceId = @ProcessInstanceId;";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessInstanceId", ProcessInstanceId },
            { "@Status", status }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    public async Task<bool> UpdateProcessInstanceAsync(ProcessInstance processInstance)
    {
        var query = @"
        UPDATE ProcessInstances
        SET 
            Name = @Name,
            Status = @Status,
            StartedBy = @StartedBy,
            StartedAt = @StartedAt,
            CompletedAt = @CompletedAt,
            Payload = @Payload,
            UpdatedAt = NOW()
        WHERE ProcessInstanceId = @ProcessInstanceId;";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessInstanceId", processInstance.Id },
            { "@Name", processInstance.Name },
            { "@Status", processInstance.Status },
            { "@StartedBy", processInstance.StartedBy },
            { "@StartedAt", processInstance.StartedAt },
            { "@CompletedAt", processInstance.CompletedAt.HasValue ? (object)processInstance.CompletedAt.Value : DBNull.Value }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(processInstance.Payload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);

        return true;
    }


    public async Task<bool> DeleteProcessInstanceAsync(Guid ProcessInstanceId)
    {
        var query = "DELETE FROM ProcessInstances WHERE ProcessInstanceId = @ProcessInstanceId;";
        var parameters = new Dictionary<string, object>
        {
            { "@ProcessInstanceId", ProcessInstanceId }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }
    #endregion

    #region Process Step Instances

    public async Task<ProcessStepInstance> CreateProcessStepInstanceAsync(ProcessStepInstance stepInstance)
    {
        stepInstance.Id = Guid.NewGuid();

        var query = @"
            INSERT INTO ProcessStepInstances (ProcessStepInstanceId, ProcessInstanceId, ProcessStepId, Status, Payload, Name)
            VALUES (@ProcessStepInstanceId, @ProcessInstanceId, @ProcessStepId, @Status, @Payload, @Name);";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessStepInstanceId", stepInstance.Id },
            { "@Name", stepInstance.Name },
            { "@ProcessInstanceId", stepInstance.ProcessInstanceId },
            { "@ProcessStepId", stepInstance.ProcessStepId },
            { "@Status", stepInstance.Status }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(stepInstance.Payload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);

        return stepInstance;
    }

    public async Task<ProcessStepInstance> GetProcessStepInstanceAsync(Guid stepInstanceId)
    {
        var query = "SELECT * FROM ProcessStepInstances WHERE ProcessStepInstanceId = @ProcessStepInstanceId;";
        var parameters = new Dictionary<string, object>
        {
            { "@ProcessStepInstanceId", stepInstanceId }
        };

        var stepInstance = await ExecuteQuerySingleAsync<ProcessStepInstance>(query, parameters);
        return stepInstance;
    }

    public async Task<bool> UpdateProcessStepInstanceAsync(ProcessStepInstance stepInstance)
    {
        var query = @"
        UPDATE ProcessStepInstances
        SET Name = @Name,
            Status = @Status,
            StartedAt = @StartedAt,
            CompletedAt = @CompletedAt,
            Payload = @Payload,
            UpdatedAt = NOW()
        WHERE ProcessStepInstanceId = @ProcessStepInstanceId;";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessStepInstanceId", stepInstance.Id },
            { "@Name", stepInstance.Name },
            { "@Status", stepInstance.Status },
            { "@StartedAt", stepInstance.StartedAt },
            { "@CompletedAt", stepInstance.CompletedAt }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(stepInstance.Payload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);
        return true;
    }

    public async Task<bool> UpdateProcessStepInstanceStatusAsync(Guid stepInstanceId, string status)
    {
        var query = @"
            UPDATE ProcessStepInstances
            SET Status = @Status,
                UpdatedAt = NOW(),
                StartedAt = CASE WHEN @Status = 'InProgress' AND StartedAt IS NULL THEN NOW() ELSE StartedAt END,
                CompletedAt = CASE WHEN @Status = 'Completed' THEN NOW() ELSE CompletedAt END
            WHERE ProcessStepInstanceId = @ProcessStepInstanceId;";

        var parameters = new Dictionary<string, object>
        {
            { "@ProcessStepInstanceId", stepInstanceId },
            { "@Status", status }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    public async Task<bool> DeleteProcessStepInstanceAsync(Guid stepInstanceId)
    {
        var query = "DELETE FROM ProcessStepInstances WHERE ProcessStepInstanceId = @ProcessStepInstanceId;";
        var parameters = new Dictionary<string, object>
        {
            { "@ProcessStepInstanceId", stepInstanceId }
        };

        await ExecuteQueryAsync(query, parameters);
        return true;
    }

    #endregion
}
