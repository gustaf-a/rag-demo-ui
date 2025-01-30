
namespace Shared.Services;

public interface IComputerUseService
{
    Task<string> SendTask(string taskInstructions);
}