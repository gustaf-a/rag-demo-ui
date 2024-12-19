using Shared.Models;

namespace ProcessDemo.Processes
{
    public interface IProcessExecutor
    {
        Task RunProcessInstance(ProcessInstance processInstance);
    }
}