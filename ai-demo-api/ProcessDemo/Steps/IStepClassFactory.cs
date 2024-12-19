using ProcessDemo.Steps.StepImplementations;

namespace Shared.Search;

public interface IStepClassFactory
{
    IProcessStep Create(string stepClassName);
}