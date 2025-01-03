using Shared.Models;
using Microsoft.Extensions.Logging;
using Shared.Repositories;
using Shared.Generation.LlmServices;

namespace ProcessDemo.Steps.StepImplementations;

public class ChatCompletionStep(ILlmServiceFactory _llmServiceFactory, ILogger<ProcessStepBase> logger, IProcessRepository processRepository) : ProcessStepBase(logger), IProcessStep
{
    public string StepClassName => nameof(ChatCompletionStep);

    protected async override Task<ProcessStepExecutionResult> ExecuteInternal(ProcessStepInstance stepInstance)
    {
        var startingState = stepInstance.Payload.StartingState
            ?? throw new Exception("No starting state found.");

        var systemPrompt = (string)stepInstance.Payload.Options[ProcessPayloadKeys.SystemPrompt]
            ?? throw new Exception("SystemPrompt not found in step instance options.");

        var userPrompt = (string)startingState[ProcessPayloadKeys.UserPrompt]
            ?? throw new Exception("SystemPrompt not found in starting state.");

        List<ChatMessage> chatMessages =
        [
            new("system", systemPrompt),
            new("user", userPrompt),
        ];

        var chatOptions = new ChatOptions
        {
            Temperature = 0.2
        };

        var llmService = _llmServiceFactory.Create();

        var response = await llmService.GetChatResponse(chatMessages, chatOptions);

        stepInstance.Payload.EndingState = [];

        var content = response.ChatMessages.Last().Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            return new ProcessStepExecutionResult
            {
                IsSuccess = false,
                Payload = stepInstance.Payload,
                Message = "Failed to get chat completion.",
                Status = ProcessStatus.Failed
            };
        }

        stepInstance.Payload.EndingState.Add(ProcessPayloadKeys.Text, content);
        stepInstance.Payload.EndingState.Add(ProcessPayloadKeys.Result, content);

        return new ProcessStepExecutionResult
        {
            IsSuccess = true,
            Payload = stepInstance.Payload,
            Status = ProcessStatus.Completed
        };
    }
}
