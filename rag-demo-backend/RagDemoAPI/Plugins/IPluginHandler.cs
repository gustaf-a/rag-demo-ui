using Microsoft.SemanticKernel;

namespace RagDemoAPI.Plugins;

public interface IPluginHandler
{
    void AddPlugins(Kernel kernel, IEnumerable<string> pluginNames);
    void AddPlugins(Kernel kernel);
}
