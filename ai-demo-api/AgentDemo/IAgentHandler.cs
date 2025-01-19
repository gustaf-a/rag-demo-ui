using Shared.Models;
using Shared.Models.Agents;

namespace AgentDemo;

public interface IAgentHandler
{
    Task<IEnumerable<Agent>> GetAgents();
    Task<AgentTask> StartAgentTask(StartAgentTaskRequest startAgentTaskRequest);

    Task<IEnumerable<Agent>> GetTeams();
    Task<Agent> CreateAgent(Agent agent);
}