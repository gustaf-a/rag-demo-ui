using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Shared.Configuration;

namespace Shared.LlmServices;

public class KernelCreator(IConfiguration configuration, Kernel _kernel) : IKernelCreator
{
    private readonly AzureOptions _azureSettings = configuration.GetSection(AzureOptions.Azure).Get<AzureOptions>();

    public Kernel CreateKernelWithChatCompletion()
    {
        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            _azureSettings.ChatService.Name,
            _azureSettings.ChatService.Endpoint,
            _azureSettings.ChatService.ApiKey);

        return builder.Build();
    }

    public IChatCompletionService GetChatCompletionService()
    {
        return _kernel.GetRequiredService<IChatCompletionService>();
    }
}
