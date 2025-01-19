using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Shared.LlmServices
{
    public interface IKernelCreator
    {
        Kernel CreateKernelWithChatCompletion();
        IChatCompletionService GetChatCompletionService();
    }
}