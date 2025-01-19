using AgentDemo.TerminationStrategies;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.History;
using Shared.Extensions;
using Shared.LlmServices;
using Shared.Models.Agents;
using System.Text.Json;

namespace AgentDemo.Agents;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class TerminationStrategyFactory(IKernelCreator _kernelCreator, IAgentFactory _agentFactory) : ITerminationStrategyFactory
{
    private const int _defaultMaxIterations = 10;

    private const string _taskFinishedString = "Task finished";

    public async Task<TerminationStrategy> Create()
    {
        return await Create(null);
    }

    public async Task<TerminationStrategy> Create(TerminationStrategyInfo terminationStrategyInfo)
    {
        if (terminationStrategyInfo is null)
            throw new NotSupportedException("Termination without agents not supported.");

        return terminationStrategyInfo.Type switch
        {
            nameof(TerminationStrategyTypes.PromptFunktion)
                => await CreateAgentBasedPromptFunctionStrategy(terminationStrategyInfo),
            _
                => throw new NotSupportedException($"Unsupported termination strategy type: {terminationStrategyInfo.Type}."),
        };
    }

    private const string _historyParameterNames = "history";

    private async Task<TerminationStrategy> CreateAgentBasedPromptFunctionStrategy(TerminationStrategyInfo terminationStrategyInfo)
    {
        var maxIterations = terminationStrategyInfo.Payload.TryGetValue(TerminationStrategyKeys.MaxIterations, out var maxIterationsValue)
                    ? (int)maxIterationsValue
                    : _defaultMaxIterations;

        if (!terminationStrategyInfo.Payload.TryGetValue(TerminationStrategyKeys.Agents, out object? agentsObject) || agentsObject is null)
            throw new Exception($"Failed to get value of {TerminationStrategyKeys.Agents}.");

        List<Guid> terminationAgentIds = agentsObject is JsonElement jsonElement 
            ? jsonElement.Deserialize<List<Guid>>() 
            : (List<Guid>)agentsObject;
        
        //The agents which are allowed to approve.
        var terminationAgents = await _agentFactory.Create(terminationAgentIds);
        if (terminationAgents.IsNullOrEmpty())
            throw new Exception($"Failed to create terminationagents: {string.Join(',', terminationAgentIds)}");

        //TODO Replace history with parameter and Task finished with variable from options.
        KernelFunction terminationFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy(
                """
                Determine if the task has been finished. If so, respond with the phrase: 'Task finished'.

                History:
                {{$history}}
                """,
                safeParameterNames: _historyParameterNames);

        var historyReducer = GetHistoryReducer(terminationStrategyInfo);

        var kernelFunctionTerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, _kernelCreator.CreateKernelWithChatCompletion())
        {
            Agents = terminationAgents.ToArray(),
            ResultParser = GetResultParser(),
            // The prompt variable name for the history argument.
            HistoryVariableName = "history",
            MaximumIterations = maxIterations,
            HistoryReducer = historyReducer,
        };

        return kernelFunctionTerminationStrategy;
    }

    private const int _reduceHistoryBySummarizationDefaultN = 2;
    private const int _reduceHistoryBySummarizationDefaultThreshold = 2;

    private IChatHistoryReducer GetHistoryReducer(TerminationStrategyInfo terminationStrategyInfo)
    {
        if (terminationStrategyInfo.Payload.TryGetValue(TerminationStrategyKeys.ReduceHistoryTruncateToMostRecentNMessages, out object? value))
            return new ChatHistoryTruncationReducer((int)value);

        if (terminationStrategyInfo.Payload.ContainsKey(TerminationStrategyKeys.ReduceHistoryBySummarization))
        {
            return new ChatHistorySummarizationReducer(
                _kernelCreator.GetChatCompletionService(),
                _reduceHistoryBySummarizationDefaultN,
                _reduceHistoryBySummarizationDefaultThreshold);
        }

        return null;
    }

    private static Func<FunctionResult, bool> GetResultParser()
    {
        //TODO create more advanced?

        return (result) => result.GetValue<string>()?.Contains(_taskFinishedString, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
