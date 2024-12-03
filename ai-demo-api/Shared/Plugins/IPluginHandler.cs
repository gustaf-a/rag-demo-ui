using Microsoft.SemanticKernel;

namespace Shared.Plugins;

public interface IPluginHandler
{
    void AddPlugins(Kernel kernel, IEnumerable<string> pluginNames);
    void AddPlugins(Kernel kernel);
}
