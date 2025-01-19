using Microsoft.Extensions.Configuration;
using Shared.Models;
using Shared.Models.Agents;
using System.Text.Json;

namespace Shared.Repositories;

public class AgentRepository(IConfiguration configuration)
    : RepositoryBase(configuration), IAgentRepository
{
    #region Agents

    /// <summary>
    /// Creates a new Agent in the database.
    /// </summary>
    public async Task<Agent> CreateAgentAsync(Agent agent)
    {
        agent.Id = agent.Id == Guid.Empty ? Guid.NewGuid() : agent.Id;

        var query = @"
            INSERT INTO Agents (Id, Name, Description, Options)
            VALUES (@Id, @Name, @Description, @Options);
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", agent.Id },
            { "@Name", agent.Name },
            { "@Description", agent.Description },
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Options", JsonSerializer.Serialize(agent.Options) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);

        return agent;
    }

    /// <summary>
    /// Gets a single Agent by its ID.
    /// </summary>
    public async Task<Agent> GetAgentByIdAsync(Guid agentId)
    {
        var query = @"
            SELECT *
            FROM Agents
            WHERE Id = @Id;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", agentId }
        };

        var agentModels = await ExecuteQueryAsync<AgentModel>(query, parameters);

        var agentModel = agentModels.SingleOrDefault();
        if (agentModel == null)
            return null;

        return await GetAgentInfo(agentModel);
    }

    /// <summary>
    /// Gets a single Agent by Name.
    /// </summary>
    public async Task<Agent> GetAgentByNameAsync(string agentName)
    {
        var query = @"
            SELECT *
            FROM Agents
            WHERE Name = @Name;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Name", agentName }
        };

        var agentModels = await ExecuteQueryAsync<AgentModel>(query, parameters);

        var agentModel = agentModels.SingleOrDefault();
        if (agentModel == null)
            return null;

        return await GetAgentInfo(agentModel);
    }

    /// <summary>
    /// Updates an existing Agent in the database.
    /// </summary>
    public async Task<Agent> UpdateAgentAsync(Agent agent)
    {
        var query = @"
            UPDATE Agents
            SET Name = @Name,
                Description = @Description,
                Options = @Options
            WHERE Id = @Id;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", agent.Id },
            { "@Name", agent.Name },
            { "@Description", agent.Description },
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Options", JsonSerializer.Serialize(agent.Options) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);

        return agent;
    }

    /// <summary>
    /// Deletes an Agent by its ID.
    /// </summary>
    public async Task DeleteAgentAsync(Guid agentId)
    {
        var query = @"
            DELETE FROM Agents
            WHERE Id = @Id;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", agentId }
        };

        await ExecuteQueryAsync(query, parameters);
    }

    /// <summary>
    /// Returns all Agents in the database.
    /// </summary>
    public async Task<IEnumerable<Agent>> GetAllAgentsAsync()
    {
        var query = "SELECT * FROM Agents;";

        var agentModels = await ExecuteQueryAsync<AgentModel>(query, new Dictionary<string, object>());
        var agentList = new List<Agent>();

        foreach (var agentModel in agentModels)
        {
            var agent = await GetAgentInfo(agentModel);
            agentList.Add(agent);
        }

        return agentList;
    }

    /// <summary>
    /// Returns all Agents that match the provided list of IDs.
    /// </summary>
    public async Task<IEnumerable<Agent>> GetAllAgentsAsync(List<Guid> agentIds)
    {
        var inClause = string.Join(",", agentIds.Select((_, i) => $"@Id{i}"));

        var query = $@"
            SELECT *
            FROM Agents
            WHERE Id IN ({inClause});
        ";

        var parameters = new Dictionary<string, object>();
        for (int i = 0; i < agentIds.Count; i++)
        {
            parameters.Add($"@Id{i}", agentIds[i]);
        }

        var agentModels = await ExecuteQueryAsync<AgentModel>(query, parameters);
        var agentList = new List<Agent>();

        foreach (var agentModel in agentModels)
        {
            var agent = await GetAgentInfo(agentModel);
            agentList.Add(agent);
        }

        return agentList;
    }

    /// <summary>
    /// Helper to convert an AgentModel to the domain Agent object.
    /// If you need to load related data or do additional transformations, handle it here.
    /// </summary>
    private async Task<Agent> GetAgentInfo(AgentModel agentModel)
    {
        var agent = new Agent
        {
            Id = agentModel.Id,
            Name = agentModel.Name,
            Description = agentModel.Description,
            Options = JsonSerializer.Deserialize<Dictionary<string, string>>(agentModel.Options) 
                        ?? throw new Exception($"Failed to get agent options for {agentModel.Id}.")
        };

        return agent;
    }

    #endregion

    #region AgentTasks

    public async Task<AgentTask> CreateAgentTaskAsync(AgentTask agentTask)
    {
        var query = @"
            INSERT INTO AgentTasks 
            (Id, Name, TaskPrompt, Status, Payload, CompletedAt, StartedAt, UpdatedAt)
            VALUES
            (@Id, @Name, @TaskPrompt, @Status, @Payload, @CompletedAt, @StartedAt, @UpdatedAt);
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", agentTask.Id },
            { "@Name",      agentTask.Name },
            { "@TaskPrompt",agentTask.TaskPrompt },
            { "@Status",    agentTask.Status },
            { "@CompletedAt", agentTask.CompletedAt },
            { "@StartedAt",   agentTask.StartedAt },
            { "@UpdatedAt",   agentTask.UpdatedAt }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(agentTask.Payload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);

        return agentTask;
    }

    /// <summary>
    /// Gets an AgentTask by its ID.
    /// </summary>
    public async Task<AgentTask> GetAgentTaskAsync(Guid Id)
    {
        var query = @"
            SELECT *
            FROM AgentTasks
            WHERE Id = @Id;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", Id }
        };

        var agentTaskModels = await ExecuteQueryAsync<AgentTaskModel>(query, parameters);

        var agentTaskModel = agentTaskModels.Single();
        if (agentTaskModel is null) 
            return null;

        return new AgentTask
        {
            Id = agentTaskModel.Id,
            Name = agentTaskModel.Name,
            TaskPrompt = agentTaskModel.TaskPrompt,
            Status = agentTaskModel.Status,
            Payload = JsonSerializer.Deserialize<AgentTaskPayload>(agentTaskModel.Payload)
                        ?? throw new Exception($"Failed to get agent task payload for {agentTaskModel.Id}."),
            CompletedAt = agentTaskModel.CompletedAt,
            StartedAt = agentTaskModel.StartedAt,
            UpdatedAt = agentTaskModel.UpdatedAt
        };
    }

    /// <summary>
    /// Updates the entire AgentTask record (all fields).
    /// </summary>
    public async Task<AgentTask> UpdateAgentTaskAsync(AgentTask agentTask)
    {
        agentTask.UpdatedAt = DateTime.UtcNow;

        var query = @"
            UPDATE AgentTasks
            SET Name = @Name,
                TaskPrompt = @TaskPrompt,
                Status = @Status,
                Payload = @Payload,
                CompletedAt = @CompletedAt,
                StartedAt = @StartedAt,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", agentTask.Id },
            { "@Name",        agentTask.Name },
            { "@TaskPrompt",  agentTask.TaskPrompt },
            { "@Status",      agentTask.Status },
            { "@CompletedAt", agentTask.CompletedAt },
            { "@StartedAt",   agentTask.StartedAt },
            { "@UpdatedAt",   agentTask.UpdatedAt }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(agentTask.Payload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);

        return agentTask;
    }

    /// <summary>
    /// Updates only the status of an AgentTask.
    /// </summary>
    public async Task UpdateAgentTaskStatusAsync(Guid Id, string newStatus)
    {
        var query = @"
            UPDATE AgentTasks
            SET Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", Id },
            { "@Status",      newStatus },
            { "@UpdatedAt",   DateTime.UtcNow }
        };

        await ExecuteQueryAsync(query, parameters);
    }

    /// <summary>
    /// Updates only the Payload of an AgentTask.
    /// </summary>
    public async Task UpdateAgentTaskPayloadAsync(Guid Id, AgentTaskPayload newPayload)
    {
        var query = @"
            UPDATE AgentTasks
            SET Payload = @Payload,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
        ";

        var parameters = new Dictionary<string, object>
        {
            { "@Id", Id },
            { "@UpdatedAt",   DateTime.UtcNow }
        };

        var jsonBParameters = new Dictionary<string, string>
        {
            { "@Payload", JsonSerializer.Serialize(newPayload) }
        };

        await ExecuteQueryAsync(query, parameters, jsonBParameters);
    }

    #endregion
}
