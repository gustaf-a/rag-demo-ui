using Shared.Models;

namespace AgentDemo;

public interface IAgentTaskExecutor
{
    Task RunAgentTask(AgentTask agentTask);
}