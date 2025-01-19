using Shared.Extensions;
using Shared.Models;
using Shared.Models.Agents;
using Shared.Repositories;
using System.Text.Json;

namespace AgentDemo;

public class AgentHandler(IAgentRepository _agentTaskRepository, IAgentTaskExecutor _agentTaskExecutor) : IAgentHandler
{
    public async Task<Agent> CreateAgent(Agent agent)
    {
        //TODO Ensure no spaces in agent name

        var createdAgent = await _agentTaskRepository.CreateAgentAsync(agent);

        return createdAgent;
    }

    public async Task<IEnumerable<Agent>> GetAgents()
    {
        var agents = await _agentTaskRepository.GetAllAgentsAsync();

        return agents;
    }

    public Task<IEnumerable<Agent>> GetTeams()
    {
        throw new NotImplementedException();
    }

    public async Task<AgentTask> StartAgentTask(StartAgentTaskRequest startAgentTaskRequest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startAgentTaskRequest.TaskPrompt);

        if (startAgentTaskRequest.Agents.IsNullOrEmpty())
            throw new ArgumentException("At least one agent must be assigned to solve a task.");
        
        var payload = await CreateAgentTaskPayload(startAgentTaskRequest);

        var agentTaskToCreate = new AgentTask
        {
            Id = Guid.NewGuid(),
            Name = startAgentTaskRequest.Name, //TODO generate if not.
            TaskPrompt = startAgentTaskRequest.TaskPrompt,
            Status = AgentTaskStatus.NotStarted,
            Payload = payload,
            UpdatedAt = DateTime.UtcNow
        };

        var agentTask = await _agentTaskRepository.CreateAgentTaskAsync(agentTaskToCreate);

        try
        {
            await _agentTaskExecutor.RunAgentTask(agentTask);
        }
        catch (Exception ex)
        {
            var agentTaskWithUpdates = await _agentTaskRepository.GetAgentTaskAsync(agentTask.Id);

            agentTaskWithUpdates.Status = AgentTaskStatus.Failed;

            agentTaskWithUpdates.Payload.AddError(ex);

            await _agentTaskRepository.UpdateAgentTaskAsync(agentTask);
        }

        return agentTask;
    }

    private async Task<AgentTaskPayload> CreateAgentTaskPayload(StartAgentTaskRequest startAgentTaskRequest)
    {
        var payload = new AgentTaskPayload
        {
            Agents = await GetAgentGuids(startAgentTaskRequest.Agents)
        };

        foreach (var options in startAgentTaskRequest.Options)
            payload.State.Add(options.Key, options.Value);

        if (startAgentTaskRequest.TerminationStrategyInfo is not null)
        {
            var terminationStrategyInfo = startAgentTaskRequest.TerminationStrategyInfo;

            if (terminationStrategyInfo.Payload.TryGetValue(TerminationStrategyKeys.Agents, out object? value))
            {
                if (value is not JsonElement jsonElement)
                    throw new Exception($"TerminationStrategyInfo payload value {TerminationStrategyKeys.Agents} is not a valid json element.");

                var agents = jsonElement.Deserialize<List<string>>();
                if (agents != null)
                    terminationStrategyInfo.Payload[TerminationStrategyKeys.Agents] = await GetAgentGuids(agents);
            }

            payload.State.Add(AgentTaskPayloadKeys.TerminationStrategyInfo, terminationStrategyInfo);
        }

        return payload;
    }

    private async Task<List<Guid>> GetAgentGuids(List<string> agentStrings)
    {
        var agentGuids = new List<Guid>();

        foreach (var agentString in agentStrings)
        {
            if (string.IsNullOrWhiteSpace(agentString))
                continue;

            if (Guid.TryParse(agentString, out var guid))
            {
                agentGuids.Add(guid);
                continue;
            }

            var agent = await _agentTaskRepository.GetAgentByNameAsync(agentString) 
                ?? throw new Exception($"Failed to resolve agent by id or name from string: {agentString}.");

            agentGuids.Add(agent.Id);
        }

        return agentGuids;
    }
}
