using ProcessDemo.Steps.StepImplementations;

namespace Shared.Search;

public class StepClassFactory(IEnumerable<IProcessStep> _processSteps) : IStepClassFactory
{
    public IProcessStep Create(string stepClassName)
    {
        return _processSteps.FirstOrDefault(p => p.StepClassName.Equals(stepClassName, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new Exception($"Process step class not found: {stepClassName}.");
    }
}