using Microsoft.SemanticKernel.Agents;

namespace AgentDemo.Agents;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public interface IAgentFactory
{
    Task<IEnumerable<ChatCompletionAgent>> Create(List<Guid> agentIds);
    Task<ChatCompletionAgent> Create(Guid agentId);
}