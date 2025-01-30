using Microsoft.SemanticKernel;
using Shared.Services;
using System.ComponentModel;

namespace Shared.Plugins;

public class ComputerUsePlugin(IComputerUseService _computerUseService) : IPlugin
{
    [KernelFunction("run_task")]
    [Description("Sends a task to the computer. The task can be complex with many steps.")]
    public async Task<string> SendTaskToQueue(
        [Description("The task instructions to be sent to the computer. Can be complex with many steps.")] string taskInstructions)
    {
        if (string.IsNullOrWhiteSpace(taskInstructions))
        {
            return "Invalid task instructions. No input provided.";
        }

        try
        {
            var result = await _computerUseService.SendTask(taskInstructions);

            return result;
        }
        catch (Exception ex)
        {
            return $"Failed to send the task to the computer. Please tell the user about this exception: {ex.Message}.";
        }
    }
}
