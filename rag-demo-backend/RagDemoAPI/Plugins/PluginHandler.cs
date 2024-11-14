using Microsoft.SemanticKernel;

namespace RagDemoAPI.Plugins;

public class PluginHandler : IPluginHandler
{
    public void AddPlugins(Kernel kernel, IEnumerable<string> pluginNames)
    {
        foreach (var pluginName in pluginNames)
        {
            switch (pluginName)
            {
                case nameof(DatePlugin):
                    kernel.Plugins.AddFromType<DatePlugin>();
                    break;
                case nameof(TimePlugin):
                    kernel.Plugins.AddFromType<TimePlugin>();
                    break;
                default:
                    throw new NotImplementedException($"Plugin name not added to {nameof(PluginHandler)}: {pluginName}");
            }
        }
    }
}
