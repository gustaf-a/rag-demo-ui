using Shared.Models;
using Shared.Models.Agents;

namespace Shared.Repositories;

public interface IAgentRepository
{
    Task<Agent> CreateAgentAsync(Agent agent);
    Task<AgentTask> CreateAgentTaskAsync(AgentTask agentTask);
    Task DeleteAgentAsync(Guid agentId);
    Task<Agent> GetAgentByIdAsync(Guid agentId);
    Task<Agent> GetAgentByNameAsync(string agentName);
    Task<AgentTask> GetAgentTaskAsync(Guid agentTaskId);
    Task<IEnumerable<Agent>> GetAllAgentsAsync();
    Task<IEnumerable<Agent>> GetAllAgentsAsync(List<Guid> agentIds);
    Task<Agent> UpdateAgentAsync(Agent agent);
    Task<AgentTask> UpdateAgentTaskAsync(AgentTask agentTask);
    Task UpdateAgentTaskPayloadAsync(Guid agentTaskId, AgentTaskPayload newPayload);
    Task UpdateAgentTaskStatusAsync(Guid agentTaskId, string newStatus);
}