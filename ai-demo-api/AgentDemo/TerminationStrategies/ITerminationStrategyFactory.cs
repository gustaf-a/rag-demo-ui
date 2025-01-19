using Microsoft.SemanticKernel.Agents.Chat;
using Shared.Models.Agents;

namespace AgentDemo.TerminationStrategies;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public interface ITerminationStrategyFactory
{
    Task<TerminationStrategy> Create();
    Task<TerminationStrategy> Create(TerminationStrategyInfo terminationStrategyInfo);
}