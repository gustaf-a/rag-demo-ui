using Microsoft.SemanticKernel.Agents;
using Shared.LlmServices;
using Shared.Models.Agents;
using Shared.Repositories;

namespace AgentDemo.Agents;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class AgentFactory(IKernelCreator _kernelCreator, IAgentRepository _agentRepository) : IAgentFactory
{
    public async Task<IEnumerable<ChatCompletionAgent>> Create(List<Guid> agentIds)
    {
        var chatCompletionAgents = new List<ChatCompletionAgent>();

        foreach (var agentId in agentIds)
            chatCompletionAgents.Add(await Create(agentId));

        return chatCompletionAgents;
    }

    public async Task<ChatCompletionAgent> Create(Guid agentId)
    {
        var agent = await _agentRepository.GetAgentByIdAsync(agentId)
            ?? throw new Exception($"Failed to get agent with ID {agentId}.");

        var kernel = _kernelCreator.CreateKernelWithChatCompletion();
        //adding plugins etc

        ChatCompletionAgent chatCompletionAgent =
            new()
            {
                Instructions = agent.Options[AgentOptionsKeys.AgentSystemPrompt],
                Name = agent.Name,
                Kernel = kernel,
                Description = agent.Description
            };

        return chatCompletionAgent;
    }
}
