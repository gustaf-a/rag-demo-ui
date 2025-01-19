using AgentDemo.Agents;
using AgentDemo.TerminationStrategies;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.History;
using Shared.Extensions;
using Shared.LlmServices;
using Shared.Models;
using Shared.Models.Agents;
using Shared.Repositories;
using System.Text;

namespace AgentDemo;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class AgentTaskExecutor(ILogger<AgentTaskExecutor> _logger, IAgentRepository _agentTaskRepository, IAgentFactory _agentFactory, IKernelCreator _kernelCreator, ITerminationStrategyFactory _terminationStrategyFactory) : IAgentTaskExecutor
{
    private static readonly List<string> _doNotExecuteStatuses = [AgentTaskStatus.Completed, AgentTaskStatus.InProgress];

    public async Task RunAgentTask(AgentTask agentTask)
    {
        if (_doNotExecuteStatuses.Contains(agentTask.Status))
            return;

        var agents = await _agentFactory.Create(agentTask.Payload.Agents);

        var selectionStrategy = await CreateSelectionStrategy(agentTask.Payload, agents);

        var terminationStrategy = await CreateTerminationStrategy(agentTask.Payload);

        AgentGroupChat chat = CreateAgentGroupChat(agents, terminationStrategy, selectionStrategy);

        if (agentTask.Payload.ChatHistory.IsNullOrEmpty())
            agentTask.Payload.ChatHistory.Add(new(ChatMessageRoles.User, agentTask.TaskPrompt));
        
        foreach (var chatMessage in agentTask.Payload.ChatHistory)
            chat.AddChatMessage(chatMessage.ToSemanticKernelChatMessageContent());

        await RunAgentTaskInternal(agentTask, chat);
    }

    private static AgentGroupChat CreateAgentGroupChat(IEnumerable<Microsoft.SemanticKernel.Agents.Agent> agents, TerminationStrategy terminationStrategy, KernelFunctionSelectionStrategy selectionStrategy)
    {
        return new(agents.ToArray())
        {
            ExecutionSettings =
                    new()
                    {
                        TerminationStrategy = terminationStrategy,
                        SelectionStrategy = selectionStrategy,
                    }
        };
    }

    private async Task<KernelFunctionSelectionStrategy> CreateSelectionStrategy(AgentTaskPayload payload, IEnumerable<ChatCompletionAgent> agents)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
"""
Determine which participant takes the next turn in a conversation based on the the most recent participant.
State only the name of the participant to take the next turn.
No participant should take more than one turn in a row.

Choose only from these participants:
""");

        foreach (var agentName in agents.Select(a => a.Name))
            sb.AppendLine($"- {agentName}");

        sb.AppendLine(
$$$"""

History:
{{$history}}
""");

        var selectionFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy(
            sb.ToString(),
            safeParameterNames: "history");

        ChatCompletionAgent initialAgent = agents.First();

        if (payload.State.ContainsKey(AgentTaskPayloadKeys.InitialAgent))
        {
            var initialAgentGuid = (Guid)payload.State[AgentTaskPayloadKeys.InitialAgent];

            initialAgent = await _agentFactory.Create(initialAgentGuid);
        }

        // Limit history used for selection and termination to the most recent messages.
        ChatHistoryTruncationReducer strategyReducer = new(2);

        return new KernelFunctionSelectionStrategy(selectionFunction, _kernelCreator.CreateKernelWithChatCompletion())
        {
            InitialAgent = initialAgent,
            // Returns the entire result value as a string.
            ResultParser = (result) => result.GetValue<string>() ?? agents.FirstOrDefault().Name,
            // The prompt variable name for the history argument.
            HistoryVariableName = "history",
            // Save tokens by not including the entire history in the prompt
            HistoryReducer = strategyReducer,
            // Only include the agent names and not the message content
            //EvaluateNameOnly = true,
        };
    }

    private async Task RunAgentTaskInternal(AgentTask agentTask, AgentGroupChat chat)
    {
        var payload = agentTask.Payload;

        try
        {
            //TODO termination fails. Doesn't see Task finished. Hard code check here?
            await foreach (ChatMessageContent response in chat.InvokeAsync())
            {
                payload.ChatHistory.Add(response.ToChatMessage());

                //TODO Add complete chathistory to ChatHistoryRaw
                payload.State[AgentTaskPayloadKeys.ChatHistoryRaw] = response.Items;

                await _agentTaskRepository.UpdateAgentTaskPayloadAsync(agentTask.Id, payload);
            }

            agentTask.Status = AgentTaskStatus.Completed;
        }
        catch (Exception ex)
        {
            agentTask.Status = AgentTaskStatus.Failed;

            payload.State[AgentTaskPayloadKeys.ErrorCollection] = ex;
        }

        await _agentTaskRepository.UpdateAgentTaskAsync(agentTask);
    }

    private async Task<TerminationStrategy> CreateTerminationStrategy(AgentTaskPayload payload)
    {
        if(!payload.State.ContainsKey(AgentTaskPayloadKeys.TerminationStrategyInfo))
        {
            return await _terminationStrategyFactory.Create();
        }

        var terminationStrategyInfo = (TerminationStrategyInfo)payload.State[AgentTaskPayloadKeys.TerminationStrategyInfo];

        return await _terminationStrategyFactory.Create(terminationStrategyInfo);
    }
}
