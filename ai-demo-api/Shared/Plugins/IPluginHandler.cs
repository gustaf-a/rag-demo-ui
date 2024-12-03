using Microsoft.SemanticKernel;

namespace AiDemos.Api.Plugins;

public interface IPluginHandler
{
    void AddPlugins(Kernel kernel, IEnumerable<string> pluginNames);
    void AddPlugins(Kernel kernel);
}
